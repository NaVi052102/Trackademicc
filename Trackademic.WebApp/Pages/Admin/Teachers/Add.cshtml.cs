using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Trackademic.WebApp.Pages.Admin.Teachers
{
    public class AddModel : PageModel
    {
        // Removed: All service layer dependencies for static testing
        
        [BindProperty]
        public TeacherRegistrationViewModel TeacherInput { get; set; } = new TeacherRegistrationViewModel();
        
        public List<SelectListItem> Departments { get; set; }

        public void OnGet()
        {
            TeacherInput.UserType = "Teacher"; 
            LoadDepartments();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Reload departments to keep the dropdown populated if validation fails
            LoadDepartments(); 

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // --- STATIC LOGIC (To be replaced by database code later) ---
            if (TeacherInput.PhotoUpload != null)
            {
                // Simulate file save
                TeacherInput.ProfilePictureUrl = "path/to/saved/image.jpg"; 
            }
            
            await Task.Delay(10); // Simulate asynchronous database work
            
            TempData["Message"] = $"Teacher {TeacherInput.FirstName} {TeacherInput.LastName} successfully registered (STATIC success)!";
            TempData["MessageType"] = "success";
            
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
            };

            Departments = staticDepartments.Select(d => new SelectListItem 
            {
                Value = d.Id.ToString(), 
                Text = d.Name            
            }).ToList();
        }
    }

    // =========================================================================
    // HELPER VIEW MODELS (Initialized to avoid CS8618 warnings)
    // =========================================================================
    
    public class TeacherRegistrationViewModel
    {
        // --- USERS TABLE FIELDS ---
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(255)]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [HiddenInput]
        public string UserType { get; set; } = string.Empty; 

        // --- TEACHERS TABLE FIELDS ---
        [Required(ErrorMessage = "Teacher ID is required.")]
        [StringLength(20)]
        public string TeacherId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public long DepartmentId { get; set; } 
        
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty; 

        [Phone(ErrorMessage = "Invalid Contact Number.")]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty; 

        [Display(Name = "Profile Photo")]
        public IFormFile PhotoUpload { get; set; } = default!; 
    }
    
    public class StaticDepartmentModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}