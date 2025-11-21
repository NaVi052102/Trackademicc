using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using DataModels = Trackademic.Data.Models; // Using Data Models
using System.Security.Claims;

namespace Trackademic.WebApp.Pages.Student.Profile
{
    public class StudentProfileModel : PageModel
    {
        private readonly TrackademicDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Constructor reverted to only take Context and Environment
        public StudentProfileModel(TrackademicDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public DataModels.Student StudentProfile { get; set; } = default!;

        [BindProperty]
        public IFormFile? PhotoUpload { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long studentId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Fetch student using the ID
            var student = await _context.Students
                .Include(s => s.IdNavigation) // Assuming IdNavigation is the link to User
                .FirstOrDefaultAsync(m => m.Id == studentId);

            if (student == null) return NotFound("Student not found.");

            StudentProfile = student;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long studentId))
            {
                return RedirectToPage("/Account/Login");
            }

            var studentToUpdate = await _context.Students
                .Include(s => s.IdNavigation)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (studentToUpdate == null) return NotFound();

            // Handle Image Upload
            if (PhotoUpload != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoUpload.FileName);
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await PhotoUpload.CopyToAsync(stream);
                }
                studentToUpdate.ProfilePictureUrl = "/uploads/profiles/" + fileName;
            }

            // Update Data fields
            studentToUpdate.ContactNumber = StudentProfile.ContactNumber;
            studentToUpdate.Address = StudentProfile.Address;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";

            return RedirectToPage();
        }
    }
}