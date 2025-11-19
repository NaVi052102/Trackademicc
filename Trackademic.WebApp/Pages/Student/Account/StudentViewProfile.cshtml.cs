using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;

namespace Trackademic.WebApp.Pages.Student.Account
{
    public class StudentProfileModel : PageModel
    {
        // [BindProperty] for the form data
        [BindProperty]
        public StudentProfileViewModel Student { get; set; } = new StudentProfileViewModel();

        // This list is used for the Gender dropdown
        public List<SelectListItem> GenderOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "male", Text = "Male" },
            new SelectListItem { Value = "female", Text = "Female" },
            new SelectListItem { Value = "other", Text = "Other" }
        };

        // This is a flag used by JavaScript to keep the page in edit mode after a failed post
        public bool IsEditMode { get; set; } = false;

        // --- Data Transfer Object (DTO) synchronized with DB ---
        public class StudentProfileViewModel
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
            public DateTime DateOfBirth { get; set; } = DateTime.Today;

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
            public string GuardianAddress { get; set; } = string.Empty;

            // Account Information
            [Required]
            [StringLength(255)]
            public string Username { get; set; } = string.Empty;

            public string Role { get; set; } = "Student"; // Read-only

            // Password fields are usually for NEW passwords only, not displayed here
            // We use simple string properties to catch new password input
            public string NewPassword { get; set; } = string.Empty;

            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // --- MOCK DATA LOAD ---
            // In a real app, this would query the DB for the logged-in user's profile.
            Student = new StudentProfileViewModel
            {
                FirstName = "Arbien",
                LastName = "Armenion",
                DateOfBirth = new DateTime(2000, 5, 15),
                Gender = "male",
                ContactNumber = "09123456789",
                Email = "arbien.m@example.com",
                Address = "123 Main St, Consolacion, Cebu",
                StudentNumber = "2023-001-CPE",
                YearLevel = "3rd Year",
                CourseProgram = "BS Computer Engineering",
                GuardianName = "Jane M. Doe",
                GuardianContactInfo = "09987654321",
                GuardianAddress = "456 Side Ave, Cebu City",
                Username = "arbien.m",
                Role = "Student"
            };

            // Check if postback was due to a failed update to ensure the correct mode on load
            if (!ModelState.IsValid)
            {
                IsEditMode = true;
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error updating profile. Please correct errors below.";
                TempData["MessageType"] = "danger";
                IsEditMode = true; // Stay in edit mode if validation fails
                return Page();
            }

            // Database saving logic goes here (update both users and students tables)

            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";
            return RedirectToPage("./StudentProfile"); // Redirect back to View Mode
        }

        public IActionResult OnPostDiscardAsync()
        {
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info";
            return RedirectToPage("./StudentProfile"); // Reload to restore original data
        }
    }
}