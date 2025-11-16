using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Trackademic.WebApp.Pages.Account
{
    public class ForgotPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public ForgotPasswordInputModel Input { get; set; } = new ForgotPasswordInputModel();

        [BindProperty(SupportsGet = true)]
        public int Step { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        public ForgotPasswordModel()
        {
            // Constructor
        }

        public void OnGet()
        {
            // Page loads
        }

        public async Task<IActionResult> OnPostSendCodeAsync()
        {
            if (!ModelState.IsValid)
            {
                Step = 1;
                return Page();
            }

            string mockCode = "123456";
            TempData["ResetCode"] = mockCode;
            TempData["ResetEmail"] = Input.Email;

            return RedirectToPage(new { Step = 2 });
        }

        public async Task<IActionResult> OnPostVerifyCodeAsync()
        {
            string storedCode = TempData["ResetCode"] as string;
            string email = TempData["ResetEmail"] as string;

            TempData.Keep("ResetCode");
            TempData.Keep("ResetEmail");

            if (string.IsNullOrEmpty(storedCode) || Input.Code != storedCode)
            {
                ModelState.AddModelError("Input.Code", "The code is incorrect or has expired.");
                Step = 2;
                return Page();
            }

            return RedirectToPage(new { Step = 3 });
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            if (!ModelState.IsValid)
            {
                Step = 3;
                return Page();
            }

            string email = TempData["ResetEmail"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage(new { Step = 1 });
            }

            return RedirectToPage(new { Success = true });
        }
    }
}