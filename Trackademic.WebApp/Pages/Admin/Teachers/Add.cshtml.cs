using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Trackademic.Pages.Admin.Teachers
{
    public class AddModel : PageModel
    {
        [BindProperty]
        public TeacherViewModel Teacher { get; set; } = new TeacherViewModel();

        public void OnGet()
        {
            // This is where you would load any initial data
            // (e.g., a list of departments from the database)
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                // If the form is not valid, stay on the page
                // and show validation errors.
                return Page();
            }

            // In a real app, you would:
            // 1. Hash the Teacher.Password
            // 2. Create a new Teacher object from the ViewModel
            // 3. Save it to your database
            // 4. Redirect to a success page or the new profile

            TempData["Message"] = "Teacher registered successfully!";
            return RedirectToPage("/Index"); // Redirect to home page for now
        }
    }

    // This is the ViewModel that holds the form data
    public class TeacherViewModel
    {
        // Basic Info
        [Required]
        public string FullName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        [Required]
        [Phone]
        public string ContactNumber { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Address { get; set; }

        // Teacher Details
        [Required]
        public string TeacherId { get; set; } // Replaces StudentId
        public string Department { get; set; } // Replaces Course
        public string Position { get; set; } // Replaces YearLevel

        // Account Info
        [Required]
        public string Username { get; set; }
        public string Role { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}