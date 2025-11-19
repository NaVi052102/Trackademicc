using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Trackademic.Core.Models; // Assumed location for models

namespace Trackademic.WebApp.Pages.Student.Account
{
    public class StudentRegistrationModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            // Basic Information
            [Required(ErrorMessage = "First Name is required.")]
            [StringLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Last Name is required.")]
            [StringLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Date of Birth is required.")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            public string Gender { get; set; } = string.Empty;

            [Required]
            [Phone]
            [StringLength(20)]
            public string ContactNumber { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            // Student Details
            [Required(ErrorMessage = "Student ID/Number is required.")]
            [StringLength(20)]
            public string StudentNumber { get; set; } = string.Empty;

            [StringLength(50)]
            public string YearLevel { get; set; } = string.Empty;

            public string CourseProgram { get; set; } = string.Empty;

            // Guardian Details
            public string GuardianName { get; set; } = string.Empty;
            public string GuardianContactInfo { get; set; } = string.Empty;
            public string GuardianAddress { get; set; } = string.Empty; // Added property

            // Account Information
            [Required]
            [StringLength(255)]
            public string Username { get; set; } = string.Empty;

            public string Role { get; set; } = "Student";

            [Required]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public List<SelectListItem> GenderOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "male", Text = "Male" },
            new SelectListItem { Value = "female", Text = "Female" },
            new SelectListItem { Value = "other", Text = "Other" }
        };

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // --- Database saving logic goes here ---

            TempData["SuccessMessage"] = "Student Registration Successful!";
            return RedirectToPage("/Account/Login");
        }
    }
}