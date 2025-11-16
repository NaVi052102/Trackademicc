using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks; // <-- Make sure this is present
using Microsoft.AspNetCore.Http; // For HttpContext.Session
using Trackademic.Core.Enums;     // <-- FIX: Path to your Enums
using Trackademic.Core.Interfaces; // <-- FIX: Path to IAuthService
using Trackademic.Core.Models;

namespace Trackademic.WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        // --- FIX for Non-nullable warnings ---
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ErrorMessage { get; set; } // Added '?' to allow null
        // ------------------------------------

        public class InputModel
        {
            // --- FIX for Non-nullable warnings ---
            [Required(ErrorMessage = "School ID is required.")]
            public string SchoolID { get; set; } = string.Empty; // Added default

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty; // Added default
            // ------------------------------------
        }

        public void OnGet(string handler)
        {
            // This method just loads the page.
        }

        public async Task<IActionResult> OnPostAsync(string handler)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Your 'User.cs' file has UserType as a 'string', so we use strings.
            var userType = (handler?.ToLower() == "teacher") ? "Teacher" : "Student";

            // We now call your IAuthService
            var user = await _authService.LoginAsync(Input.SchoolID, Input.Password, userType);
            // --------------------------------------

            if (user == null)
            {
                ErrorMessage = "Invalid School ID or Password.";
                return Page();
            }

            // Set the user's session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.UserType); // It's already a string
            HttpContext.Session.SetInt32("UserId", (int)user.Id);

            if (user.UserType == "Student")
            {
                return RedirectToPage("/Student/Dashboard");
            }
            else if (user.UserType == "Teacher")
            {
                return RedirectToPage("/Teacher/Dashboard");
            }
            // --------------------------------------

            return RedirectToPage("/Index");
        }
    }
}