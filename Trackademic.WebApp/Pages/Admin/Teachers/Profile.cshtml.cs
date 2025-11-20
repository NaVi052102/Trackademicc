using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Trackademic.Data.Data;
using Trackademic.Data.Models;
using BCrypt.Net;

namespace Trackademic.WebApp.Pages.Admin.Teachers
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
        public TeacherProfileViewModel Teacher { get; set; } = new TeacherProfileViewModel();

        // Stores the ID of the teacher being edited
        [BindProperty]
        public long TargetId { get; set; }

        // Dropdown for Department selection
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            if (id == 0) return RedirectToPage("./Manage");

            TargetId = id;

            // 1. Fetch Teacher + User Info
            var teacherEntity = await _context.Teachers
                .Include(t => t.IdNavigation) // User Table
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacherEntity == null)
            {
                TempData["Message"] = "Teacher not found.";
                TempData["MessageType"] = "danger";
                return RedirectToPage("./Manage");
            }

            // 2. Load Departments for Dropdown
            Departments = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName })
                .ToListAsync();

            // 3. Map to ViewModel
            Teacher = new TeacherProfileViewModel
            {
                TeacherId = teacherEntity.TeacherId,
                FirstName = teacherEntity.FirstName,
                LastName = teacherEntity.LastName,

                // Convert DateOnly? -> DateTime?
                DateOfBirth = teacherEntity.DateOfBirth.HasValue
                    ? teacherEntity.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                    : null,

                ContactNumber = teacherEntity.ContactNumber,
                Email = teacherEntity.Email,
                Address = teacherEntity.Address,
                DepartmentId = teacherEntity.DepartmentId,
                ProfilePictureUrl = teacherEntity.ProfilePictureUrl,

                // Account Info
                Username = teacherEntity.IdNavigation.Username,
                Role = teacherEntity.IdNavigation.UserType
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (TargetId == 0) return RedirectToPage("./Manage");

            if (!ModelState.IsValid)
            {
                // Reload dropdowns if validation fails
                Departments = await _context.Departments
                    .OrderBy(d => d.DeptName)
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName })
                    .ToListAsync();

                TempData["Message"] = "Please correct the errors in the form.";
                TempData["MessageType"] = "danger";
                return Page();
            }

            var teacherEntity = await _context.Teachers
                .Include(t => t.IdNavigation)
                .FirstOrDefaultAsync(t => t.Id == TargetId);

            if (teacherEntity == null) return NotFound();

            // 1. Duplicate Checks
            // Check Username
            if (teacherEntity.IdNavigation.Username != Teacher.Username)
            {
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == Teacher.Username && u.Id != TargetId);
                if (usernameExists)
                {
                    ModelState.AddModelError("Teacher.Username", "Username is taken.");
                    return Page();
                }
            }

            // 2. Handle Photo Upload
            if (Teacher.PhotoUpload != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "teachers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Delete old photo
                if (!string.IsNullOrEmpty(teacherEntity.ProfilePictureUrl))
                {
                    string oldPath = Path.Combine(_environment.WebRootPath, teacherEntity.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Teacher.PhotoUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Teacher.PhotoUpload.CopyToAsync(fileStream);
                }

                teacherEntity.ProfilePictureUrl = "/uploads/teachers/" + uniqueFileName;
                Teacher.ProfilePictureUrl = teacherEntity.ProfilePictureUrl;
            }

            // 3. Update Fields
            teacherEntity.FirstName = Teacher.FirstName;
            teacherEntity.LastName = Teacher.LastName;

            // Convert DateTime? -> DateOnly?
            teacherEntity.DateOfBirth = Teacher.DateOfBirth.HasValue
                ? DateOnly.FromDateTime(Teacher.DateOfBirth.Value)
                : null;

            teacherEntity.ContactNumber = Teacher.ContactNumber;
            teacherEntity.Email = Teacher.Email;
            teacherEntity.Address = Teacher.Address;
            teacherEntity.DepartmentId = Teacher.DepartmentId;

            // Update User Account
            teacherEntity.IdNavigation.Username = Teacher.Username;

            // 4. Password Change
            if (!string.IsNullOrWhiteSpace(Teacher.NewPassword))
            {
                if (Teacher.NewPassword != Teacher.ConfirmNewPassword)
                {
                    ModelState.AddModelError("Teacher.ConfirmNewPassword", "Passwords do not match.");
                    return Page();
                }
                teacherEntity.IdNavigation.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Teacher.NewPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Teacher profile updated successfully!";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher profile");
                TempData["Message"] = "Database error occurred.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToPage(new { id = TargetId });
        }

        public IActionResult OnPostDiscard()
        {
            TempData["Message"] = "Changes discarded.";
            TempData["MessageType"] = "info";
            return RedirectToPage(new { id = TargetId });
        }
    }

    public class TeacherProfileViewModel
    {
        [Required]
        [Display(Name = "Teacher ID")]
        public string TeacherId { get; set; }

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

        // Foreign Key
        [Required]
        public long DepartmentId { get; set; }

        // --- Account ---
        [Required]
        public string Username { get; set; }
        public string Role { get; set; }

        // --- Password Change ---
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