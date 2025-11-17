using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Trackademic.Pages.Admin.Students
{
    // This is the main PageModel class
    public class ProfileModel : PageModel
    {
        // [BindProperty] tells the page to connect this object to the form
        [BindProperty]
        public StudentProfileViewModel Student { get; set; } = new StudentProfileViewModel();

        // This runs when the page is first loaded
        public void OnGet()
        {
            // In a real app, you would load this data from your database
            // using a student ID (e.g., from the logged-in user)
            Student = new StudentProfileViewModel
            {
                FullName = "Arbien M. Armenion",
                DateOfBirth = new DateTime(2000, 5, 15),
                Gender = "Male",
                ContactNumber = "09123456789",
                EmailAddress = "arbien.armenion@example.com",
                Address = "123 Main St, Consolacion, Cebu",
                StudentId = "TRK-001-2023",
                YearLevel = "3rd Year",
                Course = "BS Computer Engineering",
                Guardian = "Jane Doe (09987654321)",
                Username = "arbien.m",
                Role = "Student"
            };
        }

        // This runs when the "Update Profile" button is clicked
        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
            {
                // If the form has errors (e.g., missing name),
                // set an error message and stay on the page.
                TempData["Message"] = "Error updating profile. Please check the fields.";
                TempData["MessageType"] = "danger"; // This is a class for a red alert box
                return Page();
            }

            // In a real app, you would save the "Student" object's data
            // to the database here.

            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success"; // This is a class for a green alert box
            return RedirectToPage("/Student/StudentProfile"); // Reload the page
        }

        // This runs when the "Discard Changes" button is clicked
        public IActionResult OnPostDiscard()
        {
            // Simply reload the page. This will call OnGet() again and
            // load the original, unchanged data from the database.
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info"; // This is a class for a blue alert box
            return RedirectToPage("/Student/StudentProfile");
        }
    }

    // This is a "ViewModel" - a simple class to hold the data for the page.
    public class StudentProfileViewModel
    {
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
        [Required]
        public string StudentId { get; set; }
        public string YearLevel { get; set; }
        public string Course { get; set; }
        public string Guardian { get; set; }
        [Required]
        public string Username { get; set; }
        public string Role { get; set; }
    }
}