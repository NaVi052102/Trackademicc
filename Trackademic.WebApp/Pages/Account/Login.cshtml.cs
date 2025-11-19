using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Trackademic.Data.Data;
using BCrypt.Net;

namespace Trackademic.WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(TrackademicDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "School ID or Username is required.")]
            public string SchoolID { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string handler = "Student")
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Teacher")) Response.Redirect("/Teachers/Dashboard");
                else if (User.IsInRole("Admin")) Response.Redirect("/Admin/Dashboard");
                else Response.Redirect("/Student/Dashboard");
            }

            ViewData["Role"] = handler;
        }

        public async Task<IActionResult> OnPostAsync(string handler)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string loginRole = string.IsNullOrEmpty(handler) ? "Student" : handler;

            try
            {
                long userId = 0;
                string storedPasswordHash = "";
                string fullName = "";
                string userRole = "";

                // Attempt to find User as the Selected Role (Teacher or Student)
                if (loginRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var teacher = await _context.Teachers
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.TeacherId == Input.SchoolID);

                    if (teacher != null && teacher.User != null)
                    {
                        userId = teacher.User.Id;
                        storedPasswordHash = teacher.User.PasswordHash;
                        fullName = $"{teacher.FirstName} {teacher.LastName}";
                        userRole = "Teacher";
                    }
                }
                else // Student
                {
                    var student = await _context.Students
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.StudentNumber == Input.SchoolID);

                    if (student != null && student.User != null)
                    {
                        userId = student.User.Id;
                        storedPasswordHash = student.User.PasswordHash;
                        fullName = $"{student.FirstName} {student.LastName}";
                        userRole = "Student";
                    }
                }

                // If not found yet, check if it is an ADMIN login
                // This allows Admins to login regardless of which tab (Student/Teacher) is active.
                if (userId == 0)
                {
                    // Check Users table for a matching Username where UserType is Admin
                    var adminUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == Input.SchoolID && u.UserType == "Admin");

                    if (adminUser != null)
                    {
                        userId = adminUser.Id;
                        storedPasswordHash = adminUser.PasswordHash;
                        userRole = "Admin";

                        // Fetch Admin profile for pretty name
                        var adminProfile = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userId);
                        fullName = adminProfile != null ? $"{adminProfile.FirstName} {adminProfile.LastName}" : "Administrator";
                    }
                }

                // Verify Password
                bool isPasswordValid = false;
                if (userId != 0)
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(Input.Password, storedPasswordHash);
                }

                if (!isPasswordValid)
                {
                    ErrorMessage = "Invalid Credentials.";
                    return Page();
                }

                // Create Identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, fullName),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim("SchoolID", Input.SchoolID)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation($"User {Input.SchoolID} logged in as {userRole}.");

                // Redirect based on Role
                if (userRole == "Teacher")
                {
                    return RedirectToPage("/Teachers/Dashboard");
                }
                else if (userRole == "Admin")
                {
                    return RedirectToPage("/Admin/Dashboard");
                }
                else
                {
                    return RedirectToPage("/Student/StudentDashboard");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing login.");
                ErrorMessage = "An internal error occurred.";
                return Page();
            }
        }
    }
}