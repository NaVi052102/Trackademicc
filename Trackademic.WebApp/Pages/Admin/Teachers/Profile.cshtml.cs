using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Trackademic.Pages.Admin.Teachers
{
    // The model class must match the @model directive in TeacherProfile.cshtml
    public class ProfileModel : PageModel
    {
        // Binds the ViewModel to the form fields
        [BindProperty]
        public TeacherProfileViewModel Teacher { get; set; } = new TeacherProfileViewModel();

        // Property to hold the ID passed via the route (e.g., ?id=501)
        public int TeacherId { get; set; }

        public IActionResult OnGet(int id)
        {
            TeacherId = id;

            // In a real application, you would use 'id' to load teacher data from a database.
            // For now, we use mock data.

            // --- Mock Data Setup ---
            Teacher = id switch
            {
                // Example 1: Match the mock data used in ManageTeacherModel
                501 => new TeacherProfileViewModel
                {
                    FullName = "Dela Cruz, Maria",
                    DateOfBirth = new DateTime(1985, 10, 20),
                    Gender = "Female",
                    ContactNumber = "09171234567",
                    EmailAddress = "maria@school.edu",
                    Address = "Unit 101, Makati City, Metro Manila",
                    TeacherId = "T-22-010",
                    Position = "Professor",
                    Department = "Computer Science",
                    Username = "maria.dcruz",
                    Role = "Teacher",
                    InternalTeacherId = id // Store the internal ID for postbacks
                },
                // Example 2
                502 => new TeacherProfileViewModel
                {
                    FullName = "Reyes, Jose",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    Gender = "Male",
                    ContactNumber = "09987654321",
                    EmailAddress = "jose@school.edu",
                    Address = "Cebu City, Cebu",
                    TeacherId = "T-24-001",
                    Position = "Instructor I",
                    Department = "Mathematics",
                    Username = "jose.reyes",
                    Role = "Teacher",
                    InternalTeacherId = id
                },
                // Default: Fallback if ID is unknown
                _ => new TeacherProfileViewModel { FullName = "Unknown Teacher", InternalTeacherId = id }
            };

            // If the mock data load fails or is incomplete, you might redirect
            if (string.IsNullOrEmpty(Teacher.FullName))
            {
                // Redirect back to the list if ID is invalid
                return RedirectToPage("./ManageTeacher");
            }

            return Page();
        }

        // Handles the form submission when the "Update Profile" button is clicked
        public IActionResult OnPostUpdate()
        {
            // Check if the required fields meet the validation rules (from the ViewModel)
            if (!ModelState.IsValid)
            {
                // If validation fails, stay on the page and display errors in Edit Mode
                TempData["Message"] = "Error updating profile. Please check the fields.";
                TempData["MessageType"] = "danger";
                return Page(); // Page() re-renders the current page with validation errors
            }

            // In a real app: Save Teacher.FullName, Teacher.DateOfBirth, etc., to the database.

            // Set success message
            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";

            // Redirect to the same page (PRG pattern: Post-Redirect-Get) to clear the form state
            return RedirectToPage("./TeacherProfile", new { id = Teacher.InternalTeacherId });
        }

        // Handles the form submission when the "Discard Changes" button is clicked
        public IActionResult OnPostDiscard()
        {
            // Simply redirect to reload the page and revert any local changes
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info";
            return RedirectToPage("./TeacherProfile", new { id = Teacher.InternalTeacherId });
        }
    }

    // --- ViewModel for the Teacher Profile Data ---
    public class TeacherProfileViewModel
    {
        // Hidden field to store the internal database ID for postbacks
        [BindProperty]
        public int InternalTeacherId { get; set; }

        // Basic Information
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [Phone]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        public string EmailAddress { get; set; }

        public string Address { get; set; }

        // Teacher Details
        [Required]
        public string TeacherId { get; set; } // The ID number shown to the user (e.g., T-22-010)

        public string Position { get; set; }
        public string Department { get; set; }

        // Account Information
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        // This is typically read-only or managed by the system
        public string Role { get; set; }
    }
}