using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Trackademic.Data.Data;
using Trackademic.Data.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace Trackademic.WebApp.Pages.Admin.Students
{
    // RESTRICT TO ADMIN
    // [Authorize(Roles = "Admin")] 
    public class AddModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AddModel> _logger;

        public AddModel(
            TrackademicDbContext context,
            IWebHostEnvironment environment,
            ILogger<AddModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            StudentInput = new StudentRegistrationViewModel
            {
                Username = string.Empty,
                Password = string.Empty,
                StudentNumber = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Gender = string.Empty,
                CourseProgram = string.Empty,
                YearLevel = string.Empty,
                UserType = "Student",
                ContactNumber = string.Empty,
            };
        }

        [BindProperty]
        public StudentRegistrationViewModel StudentInput { get; set; }

        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> GenderOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YearLevelOptions { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            StudentInput = new StudentRegistrationViewModel
            {
                Username = string.Empty,
                Password = string.Empty,
                StudentNumber = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Gender = string.Empty,
                CourseProgram = string.Empty,
                YearLevel = string.Empty,
                UserType = "Student",
                ContactNumber = string.Empty,
            };
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"VALIDATION ERROR: {error.ErrorMessage}");
                    }
                }

                await LoadDropdownsAsync();
                return Page();
            }

            // Check for Duplicates
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == StudentInput.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("StudentInput.Username", "This username is already taken.");
                _logger.LogError("ERROR: Username taken.");
                await LoadDropdownsAsync();
                return Page();
            }

            bool studentIdExists = await _context.Students.AnyAsync(s => s.StudentNumber == StudentInput.StudentNumber);
            if (studentIdExists)
            {
                ModelState.AddModelError("StudentInput.StudentNumber", "This Student Number already exists.");
                _logger.LogError("ERROR: Student ID exists.");
                await LoadDropdownsAsync();
                return Page();
            }

            bool emailExists = await _context.Students.AnyAsync(s => s.Email == StudentInput.Email);
            if (emailExists)
            {
                ModelState.AddModelError("StudentInput.Email", "This email address is already registered to another student.");
                _logger.LogError("ERROR: Email duplicate found.");
                await LoadDropdownsAsync();
                return Page();
            }

            // File Upload Logic
            string? uniqueFileName = null;
            if (StudentInput.PhotoUpload != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "students");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + StudentInput.PhotoUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await StudentInput.PhotoUpload.CopyToAsync(fileStream);
                }
                StudentInput.ProfilePictureUrl = "/uploads/students/" + uniqueFileName;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // User Creation
                    var newUser = new User
                    {
                        Username = StudentInput.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(StudentInput.Password),
                        UserType = "Student"
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Student Creation
                    var newStudent = new Trackademic.Data.Models.Student
                    {
                        Id = newUser.Id,
                        StudentNumber = StudentInput.StudentNumber,
                        FirstName = StudentInput.FirstName,
                        LastName = StudentInput.LastName,
                        Email = StudentInput.Email,
                        ContactNumber = StudentInput.ContactNumber,
                        DateOfBirth = StudentInput.DateOfBirth.HasValue ? DateOnly.FromDateTime(StudentInput.DateOfBirth.Value) : (DateOnly?)null,
                        Address = StudentInput.Address,
                        ProfilePictureUrl = StudentInput.ProfilePictureUrl,
                        Gender = StudentInput.Gender,
                        YearLevel = StudentInput.YearLevel,
                        CourseProgram = StudentInput.CourseProgram
                    };
                    _context.Students.Add(newStudent);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Message"] = $"SUCCESS: Student profile created.";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("./Add");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError($"DATABASE EXCEPTION: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        _logger.LogError($"INNER EXCEPTION: {ex.InnerException.Message}");
                    }

                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                    await LoadDropdownsAsync();
                    return Page();
                }
            }
        }

        private async Task LoadDropdownsAsync()
        {
            Departments = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new SelectListItem { Value = d.DeptName, Text = d.DeptName })
                .ToListAsync();

            GenderOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Male", Text = "Male" },
                new SelectListItem { Value = "Female", Text = "Female" },
                new SelectListItem { Value = "Prefer not to say", Text = "Prefer not to say" }
            };

            YearLevelOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "1st Year", Text = "1st Year" },
                new SelectListItem { Value = "2nd Year", Text = "2nd Year" },
                new SelectListItem { Value = "3rd Year", Text = "3rd Year" },
                new SelectListItem { Value = "4th Year", Text = "4th Year" },
                new SelectListItem { Value = "5th Year", Text = "5th Year" }
            };
        }
    }

    public class StudentRegistrationViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public required string UserType { get; set; }

        [Required(ErrorMessage = "Student Number is required")]
        public required string StudentNumber { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public required string Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Course/Program is required")]
        public required string CourseProgram { get; set; }

        [Required(ErrorMessage = "Year Level is required")]
        public required string YearLevel { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Contact Number must be exactly 11 digits (e.g., 09123456789).")]
        public required string ContactNumber { get; set; }

        public string? Address { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Required(ErrorMessage = "Profile Picture is required.")]
        [Display(Name = "Profile Photo")]
        public IFormFile? PhotoUpload { get; set; }
    }
}