using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Trackademic.Data.Data;
using Trackademic.Data.Models;
using BCrypt.Net;

namespace Trackademic.WebApp.Pages.Admin.Students
{
    public class ProfileModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            TrackademicDbContext context,
            IWebHostEnvironment environment,
            ILogger<ProfileModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [BindProperty]
        public StudentProfileViewModel Student { get; set; } = new StudentProfileViewModel();

        [BindProperty]
        public long TargetId { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            if (id == 0) return RedirectToPage("./Manage");

            TargetId = id;

            var studentEntity = await _context.Students
                .Include(s => s.IdNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (studentEntity == null)
            {
                TempData["Message"] = "Student not found.";
                TempData["MessageType"] = "danger";
                return RedirectToPage("./Manage");
            }

            Student = new StudentProfileViewModel
            {
                StudentId = studentEntity.StudentNumber,
                FirstName = studentEntity.FirstName,
                LastName = studentEntity.LastName,

                // FIX 1: Convert DateOnly? (DB) to DateTime? (ViewModel)
                DateOfBirth = studentEntity.DateOfBirth.HasValue
                    ? studentEntity.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                    : null,

                ContactNumber = studentEntity.ContactNumber,
                Email = studentEntity.Email,
                Address = studentEntity.Address,
                ProfilePictureUrl = studentEntity.ProfilePictureUrl,

                Username = studentEntity.IdNavigation.Username,
                Role = studentEntity.IdNavigation.UserType
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (TargetId == 0) return RedirectToPage("./Manage");

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please correct the errors in the form.";
                TempData["MessageType"] = "danger";
                return Page();
            }

            var studentEntity = await _context.Students
                .Include(s => s.IdNavigation)
                .FirstOrDefaultAsync(s => s.Id == TargetId);

            if (studentEntity == null) return NotFound();

            if (studentEntity.IdNavigation.Username != Student.Username)
            {
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == Student.Username && u.Id != TargetId);
                if (usernameExists)
                {
                    ModelState.AddModelError("Student.Username", "Username is taken.");
                    return Page();
                }
            }

            if (Student.PhotoUpload != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "students");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(studentEntity.ProfilePictureUrl))
                {
                    string oldPath = Path.Combine(_environment.WebRootPath, studentEntity.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Student.PhotoUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Student.PhotoUpload.CopyToAsync(fileStream);
                }

                studentEntity.ProfilePictureUrl = "/uploads/students/" + uniqueFileName;
                Student.ProfilePictureUrl = studentEntity.ProfilePictureUrl;
            }

            studentEntity.FirstName = Student.FirstName;
            studentEntity.LastName = Student.LastName;

            // FIX 2: Convert DateTime? (ViewModel) to DateOnly? (DB)
            studentEntity.DateOfBirth = Student.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(Student.DateOfBirth.Value)
                : null;

            studentEntity.ContactNumber = Student.ContactNumber;
            studentEntity.Email = Student.Email;
            studentEntity.Address = Student.Address;

            studentEntity.IdNavigation.Username = Student.Username;

            if (!string.IsNullOrWhiteSpace(Student.NewPassword))
            {
                if (Student.NewPassword != Student.ConfirmNewPassword)
                {
                    ModelState.AddModelError("Student.ConfirmNewPassword", "Passwords do not match.");
                    return Page();
                }
                studentEntity.IdNavigation.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Student.NewPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Profile updated successfully!";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student profile");
                TempData["Message"] = "Database error occurred.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToPage(new { id = TargetId });
        }
    }

    public class StudentProfileViewModel
    {
        [Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "11 digits required")]
        public string ContactNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        [Required]
        public string Username { get; set; }
        public string Role { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmNewPassword { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public IFormFile? PhotoUpload { get; set; }
    }
}