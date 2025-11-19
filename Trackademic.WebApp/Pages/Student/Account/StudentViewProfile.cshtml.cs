using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

// Assuming Core.Models is where your data models are located
// using Trackademic.Core.Models; 

namespace Trackademic.WebApp.Pages.Student.Account
{
    // Rename the page model
    public class StudentProfileModel : PageModel
    {
        // Property to hold the data loaded for the profile view
        public StudentProfileDto Profile { get; set; } = new StudentProfileDto();

        // You would typically inject services here to load the data, e.g.,
        // private readonly IStudentService _studentService;

        // Data Transfer Object (DTO) for display purposes (Read-Only)
        public class StudentProfileDto
        {
            // Basic Information
            public string FirstName { get; set; } = "John";
            public string LastName { get; set; } = "Doe";
            public DateTime DateOfBirth { get; set; } = new DateTime(2000, 1, 15);
            public string Gender { get; set; } = "Male";
            public string ContactNumber { get; set; } = "0917-123-4567";
            public string Email { get; set; } = "john.doe@example.com";
            public string Address { get; set; } = "123 Academic St., Sample City";
            public string ProfilePictureUrl { get; set; } = string.Empty; // Placeholder for image path
            public string Username { get; set; } = "jdoe01";

            // Student Details (Usually non-editable)
            public string StudentNumber { get; set; } = "S-2023-12345";
            public string YearLevel { get; set; } = "3rd Year";
            public string CourseProgram { get; set; } = "BS Computer Engineering";

            // Guardian Details
            public string GuardianName { get; set; } = "Jane Doe";
            public string GuardianContactInfo { get; set; } = "0998-765-4321";
            public string GuardianAddress { get; set; } = "Same as Student";
        }

        public void OnGet()
        {
            // In a real application, this is where you load the student's data 
            // into the 'Profile' object using the student's User ID.
            // For now, it uses the hardcoded mock data above.
        }

        // You would typically have OnPost handlers for saving the edits 
        // which would redirect back to this view.
    }
}