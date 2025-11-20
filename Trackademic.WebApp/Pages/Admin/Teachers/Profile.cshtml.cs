using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trackademic.WebApp.Pages.Admin.Teachers
{
    public class ProfileModel : PageModel
    {
        // Binds the ViewModel to the form fields
        [BindProperty]
        public TeacherProfileViewModel Teacher { get; set; } = new TeacherProfileViewModel();

        // Property to hold the ID passed via the route (e.g., ?id=501)
        public int TeacherRouteId { get; set; }

        // Property to hold the Department options for the dropdown
        public List<SelectListItem> Departments { get; set; }

        public IActionResult OnGet(int id)
        {
            TeacherRouteId = id;
            
            // Load departments for the dropdown
            LoadDepartments();

            // --- Mock Data Setup ---
            Teacher = id switch
            {
                501 => new TeacherProfileViewModel
                {
                    FirstName = "Maria",
                    LastName = "Dela Cruz",
                    DateOfBirth = new DateTime(1985, 10, 20),
                    ContactNumber = "09171234567",
                    Email = "maria@school.edu",
                    Address = "Unit 101, Makati City, Metro Manila",
                    TeacherId = "T-22-010",
                    DepartmentId = 3, // Foreign Key value (e.g., Physics)
                    Username = "maria.dcruz",
                    Role = "Teacher",
                    ProfilePictureUrl = null
                },
                502 => new TeacherProfileViewModel
                {
                    FirstName = "Jose",
                    LastName = "Reyes",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    ContactNumber = "09987654321",
                    Email = "jose@school.edu",
                    Address = "Cebu City, Cebu",
                    TeacherId = "T-24-001",
                    DepartmentId = 1, // Foreign Key value (e.g., Mathematics)
                    Username = "jose.reyes",
                    Role = "Teacher",
                    ProfilePictureUrl = null
                },
                _ => new TeacherProfileViewModel { TeacherId = "INVALID" }
            };

            // If the mock data load fails or is incomplete, redirect
            if (string.IsNullOrEmpty(Teacher.FirstName))
            {
                TempData["Message"] = "Teacher not found.";
                TempData["MessageType"] = "danger";
                return RedirectToPage("./Manage");
            }

            return Page();
        }

        // Handles the form submission when the "Update Profile" button is clicked
        public async Task<IActionResult> OnPostUpdate()
        {
            // Reload departments and check model state
            LoadDepartments();
            
            // NOTE: The IFormFile property (PhotoUpload) will be checked here for a file.

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error updating profile. Please check the fields.";
                TempData["MessageType"] = "danger";
                return Page(); 
            }

            // In a real app: Save changes via a Service.

            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";

            // Redirect to the same page (PRG pattern: Post-Redirect-Get)
            return RedirectToPage("./Profile", new { id = TeacherRouteId });
        }

        // Handles the form submission when the "Discard Changes" button is clicked
        public IActionResult OnPostDiscard()
        {
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info";
            return RedirectToPage("./Profile", new { id = TeacherRouteId });
        }
        
        // Helper method to load static department data
        private void LoadDepartments()
        {
            //var staticDepartments = new List<StaticDepartmentModel>
            //{
                //new StaticDepartmentModel { Id = 1, Name = "Mathematics" },
                //new StaticDepartmentModel { Id = 2, Name = "Computer Engineering" },
                //new StaticDepartmentModel { Id = 3, Name = "Physics" },
            //};

            //Departments = staticDepartments.Select(d => new SelectListItem 
            //{
                //Value = d.Id.ToString(), 
               // Text = d.Name            
            //}).ToList();
        }
    }

    // --- ViewModel for the Teacher Profile Data (Database Aligned) ---
    public class TeacherProfileViewModel
    {
        // Name is split for DB
        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [Phone]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty; // Aligned with DB 'email'

        public string Address { get; set; } = string.Empty;

        // Teacher Details
        [Required]
        [Display(Name = "Teacher ID")]
        public string TeacherId { get; set; } = string.Empty;

        // Foreign Key
        [Required(ErrorMessage = "Department is required.")]
        public long DepartmentId { get; set; } 

        // Account Information
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;
        
        public string Role { get; set; } = string.Empty; // Read-only

        public string ProfilePictureUrl { get; set; } = string.Empty;
        
        // For file upload
        public IFormFile PhotoUpload { get; set; } = default!;
        
        // Fields for password change logic (must be added to HTML form if needed)
        // public string NewPassword { get; set; }
        // public string ConfirmNewPassword { get; set; }
    }
    
   
}