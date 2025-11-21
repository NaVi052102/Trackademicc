using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class ClassesModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public ClassesModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // Filters
        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; }
        [BindProperty(SupportsGet = true)] public string Semester { get; set; }
        [BindProperty(SupportsGet = true)] public long? ClassId { get; set; }

        [BindProperty(SupportsGet = true)] public bool IsEnrollmentMode { get; set; } = false;
        [BindProperty] public string StudentSearchTerm { get; set; }

        // Dropdown Lists
        public List<SelectListItem> ClassSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SchoolYears { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Semesters { get; set; } = new List<SelectListItem>();

        // View Data
        public ClassDetailsViewModel SelectedClass { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
        public List<StudentRosterViewModel> EnrollmentSearchResults { get; set; } = new List<StudentRosterViewModel>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync(); // Load dynamic years/semesters first

            // Set Defaults if empty
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = SchoolYears.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            await LoadPageData();
        }

        private async Task LoadDropdownsAsync()
        {
            // 1. Generate School Years (Present down to 7 years back)
            SchoolYears = new List<SelectListItem>();
            int currentYear = DateTime.Now.Year;
            if (DateTime.Now.Month < 6) currentYear--;

            for (int i = 0; i <= 7; i++)
            {
                int startYear = currentYear - i;
                string syLabel = $"{startYear}-{startYear + 1}";
                SchoolYears.Add(new SelectListItem { Value = syLabel, Text = syLabel });
            }

            // 2. Update Semesters List
            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "First", Text = "First" },
                new SelectListItem { Value = "Second", Text = "Second" },
                new SelectListItem { Value = "Summer", Text = "Summer" }
            };
            await Task.CompletedTask;
        }

        // --- EXISTING HANDLERS (Unchanged logic, just kept for completeness) ---

        public async Task<IActionResult> OnPostEnrollMode()
        {
            await LoadDropdownsAsync();
            await LoadPageData();
            IsEnrollmentMode = true;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveStudentAsync(long studentIdToRemove)
        {
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
            await LoadDropdownsAsync();
            await LoadPageData();
            IsEnrollmentMode = true;

            if (!string.IsNullOrEmpty(StudentSearchTerm))
            {
                var term = StudentSearchTerm.ToLower();
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

        public async Task<IActionResult> OnPostEnrollStudentAsync(long studentIdToEnroll)
        {
            if (!ClassId.HasValue) return Page();

            bool exists = await _context.Classenrollments
                .AnyAsync(ce => ce.ClassId == ClassId.Value && ce.StudentId == studentIdToEnroll);

            if (!exists)
            {
                var newEnrollment = new Classenrollment
                {
                    ClassId = ClassId.Value,
                    StudentId = studentIdToEnroll,
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                    EnrollmentStatus = "Enrolled"
                };
                _context.Classenrollments.Add(newEnrollment);
                await _context.SaveChangesAsync();

                var newGrade = new Grade
                {
                    EnrollmentId = newEnrollment.Id,
                    MidtermGrade = 0,
                    FinalGrade = 0,
                    FinalScore = 0
                };
                _context.Grades.Add(newGrade);
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

            // Filter classes by the Selected SchoolYear and Semester
            // Note: We need to include SchoolYear/Semester navigation to filter
            var classesQuery = _context.Classassignments
                .Where(ca => ca.TeacherId == userId)
                .Include(ca => ca.Class).ThenInclude(c => c.Subject)
                .Include(ca => ca.Class).ThenInclude(c => c.SchoolYear)
                .Include(ca => ca.Class).ThenInclude(c => c.Semester)
                .AsQueryable();

            // Apply Filters to the Dropdown population
            if (!string.IsNullOrEmpty(SchoolYear))
            {
                classesQuery = classesQuery.Where(ca => ca.Class.SchoolYear.YearName == SchoolYear);
            }
            if (!string.IsNullOrEmpty(Semester))
            {
                classesQuery = classesQuery.Where(ca => ca.Class.Semester.SemesterName.Contains(Semester));
            }

            var classes = await classesQuery
                .Select(ca => new { ca.Class.Id, Title = $"{ca.Class.Subject.SubjectCode} - {ca.Class.ClassSection}" })
                .ToListAsync();

            ClassSelectList = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title }).ToList();

            // Load Selected Class Data
            if (ClassId.HasValue && ClassId.Value > 0)
            {
                // Verify the selected class actually belongs to the filtered list (security check)
                if (classes.Any(c => c.Id == ClassId.Value))
                {
                    var cls = await _context.Classes
                        .Include(c => c.Subject)
                        .Include(c => c.Classenrollments).ThenInclude(ce => ce.Student)
                        .FirstOrDefaultAsync(c => c.Id == ClassId.Value);

                    if (cls != null)
                    {
                        SelectedClass = new ClassDetailsViewModel
                        {
                            SubjectCode = cls.Subject.SubjectCode,
                            SectionName = cls.ClassSection,
                            CourseTitle = cls.Subject.SubjectName,
                            TotalStudents = cls.Classenrollments.Count
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
                else
                {
                    // If filtered list changed and ClassId is invalid, reset it
                    ClassId = null;
                }
            }

            // Default to first class if none selected (or previous selection invalid)
            if ((!ClassId.HasValue || ClassId.Value == 0) && classes.Any())
            {
                // Removed auto-select logic to force user to pick, or uncomment below to auto-pick first
                // ClassId = classes.First().Id;
                // await LoadPageData(); // Recursive call to load details
            }
        }
    }

    public class ClassDetailsViewModel { public string SubjectCode { get; set; } public string SectionName { get; set; } public string CourseTitle { get; set; } public int TotalStudents { get; set; } }
    public class StudentRosterViewModel { public long StudentId { get; set; } public string StudentNumber { get; set; } public string FirstName { get; set; } public string LastName { get; set; } public string FullName { get; set; } }
}