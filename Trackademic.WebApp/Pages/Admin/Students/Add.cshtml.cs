using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering; // Added for SelectListItem
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Added for Linq operations

namespace Trackademic.WebApp.Pages.Admin.Students
{
    // Ensure this is restricted to Admin role in production!
    public class AddModel : PageModel
    {
        // 1. Property to bind the entire form data to the ViewModel
        [BindProperty]
        public StudentRegistrationViewModel StudentInput { get; set; }
        
        // 2. Property to hold the Department options for the dropdown
        public List<SelectListItem> Departments { get; set; }

        public void OnGet()
        {
            // Set the default user type for the account creation
            StudentInput = new StudentRegistrationViewModel { UserType = "Student" };
            
            // Load Departments (Static Data Simulation)
            LoadDepartments();
        }

        public async Task<IActionResult> OnPost()
        {
            LoadDepartments(); // Reload departments to keep the dropdown populated on error

            // 3. Perform server-side validation check
            if (!ModelState.IsValid)
            {
                // If validation fails, return to the page with error messages
                return Page();
            }

            // --- Database and File Handling Logic Goes Here ---

            // A. Handle File Upload (Simulation)
            if (StudentInput.PhotoUpload != null)
            {
                // Logic to save the file and update StudentInput.ProfilePictureUrl goes here
            }

            // B. Database Transaction: Insert into 'users' table, then 'students' table

            // For now, we simulate success
            TempData["Message"] = $"Student {StudentInput.FirstName} {StudentInput.LastName} ({StudentInput.StudentNumber}) successfully registered!";
            TempData["MessageType"] = "success"; 
            
            // 4. Redirect to prevent resubmission (Post/Redirect/Get pattern)
            return RedirectToPage("./Add");
        }
        
        // Helper method to load static department data
        private void LoadDepartments()
        {
            var staticDepartments = new List<StaticDepartmentModel>
            {
                new StaticDepartmentModel { Id = 1, Name = "Computer Engineering" },
                new StaticDepartmentModel { Id = 2, Name = "Electrical Engineering" },
                new StaticDepartmentModel { Id = 3, Name = "Civil Engineering" },
                new StaticDepartmentModel { Id = 4, Name = "General Education" },
            };

            Departments = staticDepartments.Select(d => new SelectListItem 
            {
                Value = d.Id.ToString(), 
                Text = d.Name            
            }).ToList();
        }
    }

    // =========================================================================
    // VIEW MODEL: Maps directly to form fields and database constraints
    // =========================================================================
    public class StudentRegistrationViewModel
    {
        // --- USERS TABLE FIELDS ---
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(255)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [HiddenInput]
        public string UserType { get; set; } 

        // --- STUDENTS TABLE FIELDS ---
        [Required(ErrorMessage = "Student Number is required.")]
        [StringLength(20)]
        public string StudentNumber { get; set; }
        
        [Required(ErrorMessage = "Department is required.")]
        public long DepartmentId { get; set; } // NEW FIELD
        
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Contact Number.")]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        public string ProfilePictureUrl { get; set; }

        // --- FILE UPLOAD (NOT saved to DB directly) ---
        [Display(Name = "Profile Photo")]
        public IFormFile PhotoUpload { get; set; }
    }
    
    // Simple model to hold Department data for dropdown population
    public class StaticDepartmentModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}