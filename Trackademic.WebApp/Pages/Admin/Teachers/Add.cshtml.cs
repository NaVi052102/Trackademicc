using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Trackademic.Data.Data; // Your DbContext
using Trackademic.Data.Models; // Your DB Models
using BCrypt.Net; // For Password Hashing
using Microsoft.EntityFrameworkCore;

namespace Trackademic.WebApp.Pages.Admin.Teachers
{
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
            TeacherInput = new TeacherRegistrationViewModel
            {
                UserType = "Teacher",
                Username = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                TeacherId = string.Empty,
                DepartmentId = 0,
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                ContactNumber = string.Empty,
                DateOfBirth = null,
                PhotoUpload = null!
            };
        }

        [BindProperty]
        public TeacherRegistrationViewModel TeacherInput { get; set; }

        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Validation Check
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

            // 2. Duplicate Checks

            // A. Check Username
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == TeacherInput.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("TeacherInput.Username", "This username is already taken.");
                await LoadDropdownsAsync();
                return Page();
            }

            // B. Check Teacher ID
            bool teacherIdExists = await _context.Teachers.AnyAsync(t => t.TeacherId == TeacherInput.TeacherId);
            if (teacherIdExists)
            {
                ModelState.AddModelError("TeacherInput.TeacherId", "This Teacher ID already exists.");
                await LoadDropdownsAsync();
                return Page();
            }

            // C. Check Email
            bool emailExists = await _context.Teachers.AnyAsync(t => t.Email == TeacherInput.Email);
            if (emailExists)
            {
                ModelState.AddModelError("TeacherInput.Email", "This email address is already registered.");
                await LoadDropdownsAsync();
                return Page();
            }

            // 3. Handle File Upload
            string? uniqueFileName = null;
            if (TeacherInput.PhotoUpload != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "teachers");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + TeacherInput.PhotoUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await TeacherInput.PhotoUpload.CopyToAsync(fileStream);
                }

                TeacherInput.ProfilePictureUrl = "/uploads/teachers/" + uniqueFileName;
            }

            // 4. Transactional Save
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // A. Create User
                    var newUser = new User
                    {
                        Username = TeacherInput.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(TeacherInput.Password),
                        UserType = "Teacher"
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync(); // Generates User ID

                    // B. Create Teacher Profile
                    var newTeacher = new Teacher
                    {
                        Id = newUser.Id, // Link to User
                        TeacherId = TeacherInput.TeacherId,
                        DepartmentId = TeacherInput.DepartmentId,
                        FirstName = TeacherInput.FirstName,
                        LastName = TeacherInput.LastName,
                        Email = TeacherInput.Email,
                        ContactNumber = TeacherInput.ContactNumber,
                        DateOfBirth = TeacherInput.DateOfBirth.HasValue? DateOnly.FromDateTime(TeacherInput.DateOfBirth.Value) : (DateOnly?)null,
                        Address = TeacherInput.Address,
                        ProfilePictureUrl = TeacherInput.ProfilePictureUrl
                    };

                    _context.Teachers.Add(newTeacher);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Message"] = $"SUCCESS: Teacher profile for {TeacherInput.FirstName} {TeacherInput.LastName} has been created.";
                    TempData["MessageType"] = "success";

                    return RedirectToPage("./Add");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Cleanup image if DB failed
                    if (uniqueFileName != null)
                    {
                        var path = Path.Combine(_environment.WebRootPath, "uploads", "teachers", uniqueFileName);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }

                    _logger.LogError($"DB ERROR: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving.");
                    await LoadDropdownsAsync();
                    return Page();
                }
            }
        }

        private async Task LoadDropdownsAsync()
        {
            Departments = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName })
                .ToListAsync();
        }
    }

    public class TeacherRegistrationViewModel
    {
        // --- ACCOUNT ---
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }

        public required string UserType { get; set; }

        // --- DETAILS ---
        [Required(ErrorMessage = "Teacher ID is required.")]
        public required string TeacherId { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public required long DepartmentId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Contact Number must be exactly 11 digits.")]
        public required string ContactNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public required DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        // --- UPLOAD ---
        public string? ProfilePictureUrl { get; set; }

        [Required(ErrorMessage = "Profile Picture is required.")]
        [Display(Name = "Profile Photo")]
        public required IFormFile PhotoUpload { get; set; }
    }
}