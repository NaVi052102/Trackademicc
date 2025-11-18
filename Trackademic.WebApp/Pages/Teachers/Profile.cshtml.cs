using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http; // Crucial for IFormFile
using System.Collections.Generic;
using System;
using System.Linq; 
using System.ComponentModel.DataAnnotations;

namespace Trackademic.WebApp.Pages.Teachers 
{
    public class ProfileModel : PageModel
    {
        // --- 1. Teacher Profile Fields (Editable & Bound) ---
        [BindProperty] public string TeacherId { get; set; }
        [BindProperty] [Required] public string FirstName { get; set; }
        [BindProperty] [Required] public string LastName { get; set; }
        [BindProperty] [Required] [EmailAddress] public string Email { get; set; }
        [BindProperty] public string ContactNumber { get; set; }
        [BindProperty] [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
        [BindProperty] public string Address { get; set; }

        // DEPARTMENT: Loaded for display, but NOT bound back via form post (for security)
        [Required] 
        public long DepartmentId { get; set; } 

        // --- 2. Account Information ---
        [BindProperty] [Required] public string Username { get; set; }
        public string Role { get; set; } = "Teacher"; 
        public string ProfilePictureUrl { get; set; }

        // --- 3. File Upload Property ---
        // This property binds the incoming file from the HTML form
        [BindProperty]
        public IFormFile PhotoUpload { get; set; }

        // --- 4. Password Fields (Only for post/validation) ---
        [BindProperty] [DataType(DataType.Password)] public string NewPassword { get; set; }
        [BindProperty] [DataType(DataType.Password)] 
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
        
        // --- 5. Dropdown Options ---
        public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();


        public IActionResult OnGet()
        {
            // --- Static Data Load (Replace this with DB logic later) ---
            TeacherId = "T00123";
            FirstName = "Alice";
            LastName = "Smith";
            Email = "a.smith@trackademic.edu";
            ContactNumber = "0917-555-1234";
            DateOfBirth = new DateTime(1985, 5, 20);
            Address = "123 Technology Drive, Cebu City";
            DepartmentId = 2; // Teacher is in the Science Department
            Username = "asmith";
            Role = "Teacher";
            // Example URL for testing image display
            ProfilePictureUrl = "https://cdn.iconscout.com/icon/free/png-256/free-avatar-370-456322.png"; 

            // --- Static Data for the Dropdown Options ---
            var staticDepartments = new List<DepartmentViewModel>
            {
                new DepartmentViewModel { Id = 1, Name = "Math" },
                new DepartmentViewModel { Id = 2, Name = "Science" },
                new DepartmentViewModel { Id = 3, Name = "English" },
            };

            // Convert the static list to SelectListItem format
            DepartmentOptions = staticDepartments.Select(d => new SelectListItem 
            {
                Value = d.Id.ToString(), 
                Text = d.Name            
            }).ToList();

            return Page();
        }
    }
    
    // --- Helper ViewModel for Department Options ---
    public class DepartmentViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}