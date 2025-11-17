using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Trackademic.Core.Models; // Assuming your models are here

namespace Trackademic.WebApp.Pages.Student.Account
{
    public class StudentRegistrationModel : PageModel
    {
        // [BindProperty] for the form data
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        // Data Transfer Object (DTO) synchronized with DB
        public class InputModel
        {
            // Basic Information (students table)
            [Required(ErrorMessage = "First Name is required.")]
            [StringLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Last Name is required.")]
            [StringLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Date of Birth is required.")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            public string Gender { get; set; } = string.Empty; // Maps to students.sex

            [Required]
            [Phone]
            [StringLength(20)]
            public string ContactNumber { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty; // Maps to students.email

            public string Address { get; set; } = string.Empty; // Maps to students.home_address

            // Student Details
            [Required(ErrorMessage = "Student ID/Number is required.")]
            [StringLength(20)]
            public string StudentNumber { get; set; } = string.Empty; // Maps to students.student_number

            [StringLength(50)]
            public string YearLevel { get; set; } = string.Empty;

            public string CourseProgram { get; set; } = string.Empty; // Maps to students.course_program

            public string GuardianName { get; set; } = string.Empty;
            public string GuardianContactInfo { get; set; } = string.Empty;

            // Account Information (users table)
            [Required]
            [StringLength(255)]
            public string Username { get; set; } = string.Empty; // Maps to users.username

            public string GuardianAddress { get; set; } = string.Empty;

            public string Role { get; set; } = "Student"; // Hardcoded for this form

            [Required]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // Dropdown options
        public List<SelectListItem> GenderOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "male", Text = "Male" },
            new SelectListItem { Value = "female", Text = "Female" },
            new SelectListItem { Value = "other", Text = "Other" }
        };

        public void OnGet()
        {
            // Initial load logic
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // This line will execute if any [Required] field is empty
                return Page();
            }

            // Database saving logic goes here (create user, then student)

            TempData["SuccessMessage"] = "Student Registration Successful!";
            return RedirectToPage("/Account/Login"); // Redirects to login page
        }
    }
}