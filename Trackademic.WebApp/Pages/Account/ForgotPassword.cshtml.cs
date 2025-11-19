using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using BCrypt.Net;
using Trackademic.Core.Interfaces;

namespace Trackademic.WebApp.Pages.Account
{
    public class ForgotPasswordInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly IEmailService _emailService;

        // Inject IEmailService via Constructor Injection
        public ForgotPasswordModel(
            TrackademicDbContext context,
            ILogger<ForgotPasswordModel> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        [BindProperty]
        public ForgotPasswordInputModel Input { get; set; } = new ForgotPasswordInputModel();

        [BindProperty(SupportsGet = true)]
        public int Step { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        public void OnGet()
        {
            // Reset session if starting over at Step 1
            if (Step == 1)
            {
                HttpContext.Session.Clear();
            }
        }

        // STEP 1: Send Verification Code
        public async Task<IActionResult> OnPostSendCodeAsync()
        {
            if (string.IsNullOrEmpty(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Email is required.");
                return Page();
            }

            long userId = 0;

            // 1. Search for the email in all user tables
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == Input.Email);
            if (student != null) userId = student.Id;

            if (userId == 0)
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == Input.Email);
                if (teacher != null) userId = teacher.Id;
            }

            if (userId == 0)
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == Input.Email);
                if (admin != null) userId = admin.Id;
            }

            if (userId == 0)
            {
                // Generic error to prevent email enumeration attacks
                ModelState.AddModelError("Input.Email", "Email not found in our records.");
                return Page();
            }

            // 2. Generate Secure Random Code
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 3. Store Code & UserID in Session
            HttpContext.Session.SetString("Reset_Code", code);
            HttpContext.Session.SetString("Reset_UserId", userId.ToString());
            HttpContext.Session.SetString("Reset_Expiry", DateTime.UtcNow.AddMinutes(5).ToString());

            // 4. Send Email
            try
            {
                string emailBody = $@"
                    <h3>Password Reset Request</h3>
                    <p>You requested a password reset for your Trackademic account.</p>
                    <p>Your verification code is: <h1 style='color:#1b6ec2'>{code}</h1></p>
                    <p>This code expires in 5 minutes.</p>";

                await _emailService.SendEmailAsync(Input.Email, "Trackademic Password Reset Code", emailBody);

                _logger.LogInformation($"Email sent to {Input.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email.");
                ModelState.AddModelError("", "Could not send email. Please check your configuration.");
                return Page();
            }

            Step = 2;
            return Page();
        }

        // STEP 2: Verify Code
        public async Task<IActionResult> OnPostVerifyCodeAsync()
        {
            await Task.Yield();

            string? storedCode = HttpContext.Session.GetString("Reset_Code");
            string? expiryStr = HttpContext.Session.GetString("Reset_Expiry");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(expiryStr))
            {
                ModelState.AddModelError("Input.Code", "Session expired. Please start over.");
                Step = 1;
                return Page();
            }

            if (DateTime.Parse(expiryStr) < DateTime.UtcNow)
            {
                ModelState.AddModelError("Input.Code", "Code has expired. Please try again.");
                Step = 1;
                return Page();
            }

            if (Input.Code != storedCode)
            {
                ModelState.AddModelError("Input.Code", "Invalid code.");
                Step = 2;
                return Page();
            }

            // Mark as verified so Step 3 knows it's safe to proceed
            HttpContext.Session.SetString("Reset_Verified", "true");
            Step = 3;
            return Page();
        }

        // STEP 3: Reset Password
        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            string? isVerified = HttpContext.Session.GetString("Reset_Verified");
            string? userIdStr = HttpContext.Session.GetString("Reset_UserId");

            // Verify if Step 2 was actually completed
            if (isVerified != "true" || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage(new { Step = 1 });
            }

            if (string.IsNullOrEmpty(Input.NewPassword) || Input.NewPassword.Length < 8)
            {
                ModelState.AddModelError("Input.NewPassword", "Password must be at least 8 characters.");
                Step = 3;
                return Page();
            }

            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match.");
                Step = 3;
                return Page();
            }

            long userId = long.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                // Hash and Save
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
                await _context.SaveChangesAsync();

                // Clear sensitive session data
                HttpContext.Session.Clear();
                Success = true;
                return Page();
            }
            else
            {
                ModelState.AddModelError("", "User not found.");
                Step = 1;
                return Page();
            }
        }
    }
}