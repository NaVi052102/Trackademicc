using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trackademic.Data.Data;   // FIX
using Trackademic.Data.Models; // FIX

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class ClassesModel : PageModel
    {
        private readonly TrackademicDbContext _context; // FIX

        public ClassesModel(TrackademicDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; } = "2024-2025";
        [BindProperty(SupportsGet = true)] public string Semester { get; set; } = "1st Semester";
        [BindProperty(SupportsGet = true)] public long? ClassId { get; set; }

        [BindProperty(SupportsGet = true)] public bool IsEnrollmentMode { get; set; } = false;
        [BindProperty] public string StudentSearchTerm { get; set; }

        public List<SelectListItem> ClassSelectList { get; set; } = new List<SelectListItem>();
        public ClassDetailsViewModel SelectedClass { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
        public List<StudentRosterViewModel> EnrollmentSearchResults { get; set; } = new List<StudentRosterViewModel>();

        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem> { new SelectListItem { Value = "2024-2025", Text = "2024-2025" } };
        public List<SelectListItem> Semesters { get; } = new List<SelectListItem> { new SelectListItem { Value = "1st Semester", Text = "1st Semester" } };

        public async Task OnGetAsync()
        {
            await LoadPageData();
        }

        public async Task<IActionResult> OnPostEnrollMode()
        {
            await LoadPageData();
            IsEnrollmentMode = true;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveStudentAsync(long studentIdToRemove)
        {
            // FIX: Classenrollments
            var enrollment = await _context.Classenrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == ClassId && ce.StudentId == studentIdToRemove);

            if (enrollment != null)
            {
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.EnrollmentId == enrollment.Id);
                if (grade != null) _context.Grades.Remove(grade);

                _context.Classenrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Student successfully removed.";
                TempData["MessageType"] = "success";
            }
            return RedirectToPage(new { SchoolYear, Semester, ClassId });
        }

        public async Task<IActionResult> OnPostSearchStudentAsync()
        {
            await LoadPageData();
            IsEnrollmentMode = true;

            if (!string.IsNullOrEmpty(StudentSearchTerm))
            {
                var term = StudentSearchTerm.ToLower();
                // FIX: Classenrollments
                var existingStudentIds = await _context.Classenrollments
                    .Where(ce => ce.ClassId == ClassId)
                    .Select(ce => ce.StudentId)
                    .ToListAsync();

                var matches = await _context.Students
                    .Where(s => (s.StudentNumber.Contains(term) || s.LastName.Contains(term) || s.FirstName.Contains(term))
                                && !existingStudentIds.Contains(s.Id))
                    .Take(10)
                    .ToListAsync();

                EnrollmentSearchResults = matches.Select(s => new StudentRosterViewModel
                {
                    StudentId = s.Id,
                    StudentNumber = s.StudentNumber,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    FullName = $"{s.LastName}, {s.FirstName}"
                }).ToList();
            }
            return Page();
        }

        // --- ACTION: ENROLL STUDENT ---
        public async Task<IActionResult> OnPostEnrollStudentAsync(long studentIdToEnroll)
        {
            if (!ClassId.HasValue) return Page();

            bool exists = await _context.Classenrollments
                .AnyAsync(ce => ce.ClassId == ClassId.Value && ce.StudentId == studentIdToEnroll);

            if (!exists)
            {
                // 1. Create Enrollment FIRST
                var newEnrollment = new Classenrollment
                {
                    ClassId = ClassId.Value,
                    StudentId = studentIdToEnroll,
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                    EnrollmentStatus = "Enrolled"
                };
                _context.Classenrollments.Add(newEnrollment);

                // 2. SAVE immediately to generate the Enrollment ID
                await _context.SaveChangesAsync();

                // 3. Now create the Grade using the generated ID
                var newGrade = new Grade
                {
                    EnrollmentId = newEnrollment.Id, // Now this ID is real!
                                                     // Initialize with default values to be safe
                    MidtermGrade = 0,
                    FinalGrade = 0,
                    FinalScore = 0
                };
                _context.Grades.Add(newGrade);

                // 4. Save the Grade
                await _context.SaveChangesAsync();

                TempData["Message"] = "Student successfully enrolled.";
                TempData["MessageType"] = "success";
            }

            return RedirectToPage(new { SchoolYear, Semester, ClassId, IsEnrollmentMode = false });
        }

        private async Task LoadPageData()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            // FIX: Classassignments
            var classes = await _context.Classassignments
                .Where(ca => ca.TeacherId == userId) // IMPORTANT: Ensure this matches your Teacher ID Type (long vs string)
                .Include(ca => ca.Class).ThenInclude(c => c.Subject)
                .Select(ca => new { ca.Class.Id, Title = $"{ca.Class.Subject.SubjectCode} - {ca.Class.ClassSection}" })
                .ToListAsync();

            ClassSelectList = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title }).ToList();

            if (ClassId.HasValue && ClassId.Value > 0)
            {
                var cls = await _context.Classes
                    .Include(c => c.Subject)
                    .Include(c => c.Classenrollments).ThenInclude(ce => ce.Student) // FIX: Classenrollments
                    .FirstOrDefaultAsync(c => c.Id == ClassId.Value);

                if (cls != null)
                {
                    SelectedClass = new ClassDetailsViewModel
                    {
                        SubjectCode = cls.Subject.SubjectCode,
                        SectionName = cls.ClassSection,
                        CourseTitle = cls.Subject.SubjectName,
                        TotalStudents = cls.Classenrollments.Count // FIX: Classenrollments
                    };

                    Students = cls.Classenrollments.Select(ce => new StudentRosterViewModel
                    {
                        StudentId = ce.Student.Id,
                        StudentNumber = ce.Student.StudentNumber,
                        FirstName = ce.Student.FirstName,
                        LastName = ce.Student.LastName,
                        FullName = $"{ce.Student.LastName}, {ce.Student.FirstName}"
                    }).OrderBy(s => s.FullName).ToList();
                }
            }
            else if (classes.Any())
            {
                ClassId = classes.First().Id;
                await LoadPageData();
            }
        }
    }

    public class ClassDetailsViewModel { /* Unchanged */ public string SubjectCode { get; set; } public string SectionName { get; set; } public string CourseTitle { get; set; } public int TotalStudents { get; set; } }
    public class StudentRosterViewModel { /* Unchanged */ public long StudentId { get; set; } public string StudentNumber { get; set; } public string FirstName { get; set; } public string LastName { get; set; } public string FullName { get; set; } }
}