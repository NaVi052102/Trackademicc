using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trackademic.WebApp.Pages.Admin.Students
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public StudentProfileViewModel Student { get; set; } = new StudentProfileViewModel();

        public int StudentRouteId { get; set; }

        public List<SelectListItem> Departments { get; set; }

        public IActionResult OnGet(int id)
        {
            StudentRouteId = id;
            
            // Loads static departments for the dropdown
            //LoadDepartments();

            // --- Mock Data Setup ---
            Student = id switch
            {
                101 => new StudentProfileViewModel
                {
                    FirstName = "Arbien", 
                    LastName = "Armenion", 
                    DateOfBirth = new DateTime(2000, 5, 15),
                    ContactNumber = "09123456789",
                    Email = "arbien.armenion@example.com",
                    Address = "123 Main St, Consolacion, Cebu",
                    StudentId = "TRK-001-2023",
                    Username = "arbien.m",
                    Role = "Student"
                },
                _ => new StudentProfileViewModel { StudentId = "INVALID" }
            };

            if (string.IsNullOrEmpty(Student.FirstName))
            {
                TempData["Message"] = "Student not found.";
                TempData["MessageType"] = "danger";
                return RedirectToPage("./Manage");
            }

            return Page();
        }

        public IActionResult OnPostUpdate()
        {
            // Reload departments for postback validation
            //LoadDepartments();
            
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error updating profile. Please check the fields.";
                TempData["MessageType"] = "danger";
                return Page(); 
            }

            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";

            // Redirect to the same page (PRG pattern)
            return RedirectToPage("./Profile", new { id = StudentRouteId });
        }

        public IActionResult OnPostDiscard()
        {
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info";
            return RedirectToPage("./Profile", new { id = StudentRouteId });
        }
        
        // Helper method to load static department data
        /*private void LoadDepartments()
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
        }*/
    }

    // --- ViewModel for the Student Profile Data (Database Aligned) ---
    public class StudentProfileViewModel
    {
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
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;
        public IFormFile PhotoUpload { get; set; } = default!;
    }
    
    // Helper model for department dropdown
    // This MUST BE DELETED if it is a duplicate in your project.
}