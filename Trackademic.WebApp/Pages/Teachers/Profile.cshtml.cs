using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Trackademic.WebApp.Pages.Teachers
{
    public class ProfileModel : PageModel
    {
        // --- 1. Profile Properties with Strict Validation ---

        [BindProperty]
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s.\-']+$", ErrorMessage = "First Name can only contain letters, spaces, periods, or hyphens.")]
        public string FirstName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s.\-']+$", ErrorMessage = "Last Name can only contain letters, spaces, periods, or hyphens.")]
        public string LastName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [CustomAgeValidation(18, ErrorMessage = "You must be at least 18 years old.")] // Custom Attribute below
        public DateTime? DateOfBirth { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Contact Number is required.")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Invalid format. Use 09xxxxxxxxx or +639xxxxxxxxx")]
        public string ContactNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; }

        // --- 2. Teacher Details (Read Only / Admin Set) ---
        [BindProperty] public string TeacherId { get; set; } // Readonly
        [BindProperty] public string DepartmentId { get; set; } // Disabled Dropdown
        public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>(); 

        // --- 3. Account Information ---
        [BindProperty]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 20 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and dots.")]
        public string Username { get; set; }

        [BindProperty] public string Role { get; set; } // Readonly

        // --- 4. Password Management ---
        [BindProperty]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        // --- 5. File Upload ---
        [BindProperty]
        public IFormFile PhotoUpload { get; set; }
        public string ProfilePictureUrl { get; set; }

        // ==============================================================

        public void OnGet()
        {
            LoadMockData();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            // 1. Check Model State
            if (!ModelState.IsValid)
            {
                // Reload dropdowns or auxiliary data if validation fails
                LoadDepartmentOptions();
                // Return Page to show errors
                TempData["Message"] = "Please correct the errors highlighted below.";
                TempData["MessageType"] = "danger";
                return Page();
            }

            // 2. Handle File Upload (Simulated)
            if (PhotoUpload != null)
            {
                // Validation: Check file size (e.g., max 2MB)
                if (PhotoUpload.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("PhotoUpload", "File size cannot exceed 2MB.");
                    LoadDepartmentOptions();
                    return Page();
                }
                // In real app: Save file to wwwroot/uploads and update DB path
            }

            // 3. Handle Password Change
            if (!string.IsNullOrEmpty(NewPassword))
            {
                // Logic to hash and update password
            }

            // 4. Save Changes to DB (Simulated)
            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";

            // Redirect to self to clear post data and return to View Mode
            return RedirectToPage();
        }

        public IActionResult OnPostDiscard()
        {
            return RedirectToPage();
        }

        // --- Helpers ---
        private void LoadMockData()
        {
            // Simulate fetching from DB
            FirstName = "Maria";
            LastName = "Dela Cruz";
            DateOfBirth = new DateTime(1985, 8, 15);
            ContactNumber = "09171234567";
            Email = "maria.delacruz@school.edu";
            Address = "123 Sampaguita St., Cebu City";
            TeacherId = "T-2023-001";
            DepartmentId = "1"; // Matches Dept Options
            Username = "maria.teacher";
            Role = "Teacher";
            ProfilePictureUrl = ""; // Empty for now

            LoadDepartmentOptions();
        }

        private void LoadDepartmentOptions()
        {
            DepartmentOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Computer Engineering" },
                new SelectListItem { Value = "2", Text = "Electrical Engineering" },
                new SelectListItem { Value = "3", Text = "General Education" }
            };
        }

        // --- Custom Age Validation Attribute ---
        public class CustomAgeValidation : ValidationAttribute
        {
            private int _minAge;
            public CustomAgeValidation(int minAge) { _minAge = minAge; }
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null) return ValidationResult.Success;
                DateTime dob = (DateTime)value;
                int age = DateTime.Today.Year - dob.Year;
                if (dob > DateTime.Today.AddYears(-age)) age--;
                
                if (age < _minAge) return new ValidationResult(ErrorMessage);
                return ValidationResult.Success;
            }
        }
    }
}