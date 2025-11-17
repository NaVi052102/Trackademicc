using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Trackademic.Core.Interfaces;
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

        // FIX: Input is initialized
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        // FIX: ErrorMessage is nullable
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            // FIX: Properties are initialized
            [Required(ErrorMessage = "School ID is required.")]
            public string SchoolID { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string handler)
        {
        }

        public async Task<IActionResult> OnPostAsync(string handler)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // UserType is passed as a STRING to the service
            var userType = (handler?.ToLower() == "teacher") ? "Teacher" : "Student";
            var user = await _authService.LoginAsync(Input.SchoolID, Input.Password, userType);

            if (user == null)
            {
                ErrorMessage = "Invalid School ID or Password.";
                return Page();
            }

            // Set the user's session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.UserType);
            HttpContext.Session.SetInt32("UserId", (int)user.Id);

            // FIX: Comparison is string to string
            if (user.UserType == "Student")
            {
                return RedirectToPage("/Student/Dashboard");
            }
            else if (user.UserType == "Teacher")
            {
                return RedirectToPage("/Teacher/Dashboard");
            }

            return RedirectToPage("/Index");
        }
    }
}