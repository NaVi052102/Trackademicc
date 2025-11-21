using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class ProfileModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly IWebHostEnvironment _environment; // Helper for server paths

        public ProfileModel(TrackademicDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public TeacherProfileViewModel TeacherProfile { get; set; } = new TeacherProfileViewModel();

        [TempData]
        public string Message { get; set; }

        [TempData]
        public string MessageType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Id == userId);

            if (teacher == null) return RedirectToPage("/Account/Login");

            TeacherProfile = new TeacherProfileViewModel
            {
                TeacherId = teacher.TeacherId,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Department = teacher.Department?.DeptName ?? "N/A",
                DateOfBirth = teacher.DateOfBirth,
                ProfilePictureUrl = teacher.ProfilePictureUrl, // Load existing image URL

                ContactNumber = teacher.ContactNumber,
                Address = teacher.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Note: We don't check !ModelState.IsValid strictly here because PhotoUpload is optional

            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage("/Account/Login");

            var teacher = await _context.Teachers.FindAsync(userId);
            if (teacher == null) return NotFound();

            // --- FILE UPLOAD LOGIC ---
            if (TeacherProfile.PhotoUpload != null)
            {
                // 1. Create "uploads" folder if it doesn't exist
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 2. Generate unique filename (e.g., T-2025-001_guid.jpg)
                string uniqueFileName = $"{teacher.TeacherId}_{Guid.NewGuid()}{Path.GetExtension(TeacherProfile.PhotoUpload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Save file to server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await TeacherProfile.PhotoUpload.CopyToAsync(fileStream);
                }

                // 4. Update Database Path
                teacher.ProfilePictureUrl = "/uploads/" + uniqueFileName;
            }

            // Update other fields
            teacher.ContactNumber = TeacherProfile.ContactNumber;
            teacher.Address = TeacherProfile.Address;

            await _context.SaveChangesAsync();

            Message = "Profile updated successfully.";
            MessageType = "success";

            return RedirectToPage();
        }
    }

    public class TeacherProfileViewModel
    {
        public string TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }

        public string? ProfilePictureUrl { get; set; } // Stores the URL string

        [Display(Name = "Upload Photo")]
        public IFormFile? PhotoUpload { get; set; } // Handles the actual file upload

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [Phone]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Home Address")]
        public string? Address { get; set; }
    }
}