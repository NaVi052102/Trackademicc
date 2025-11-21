using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class GradesModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public GradesModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filters ---
        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; }
        [BindProperty(SupportsGet = true)] public string Semester { get; set; }
        [BindProperty(SupportsGet = true)] public long? ClassId { get; set; }

        // --- Dropdowns ---
        public List<SelectListItem> SchoolYears { get; set; }
        public List<SelectListItem> Semesters { get; set; }
        public List<SelectListItem> ClassSelectList { get; set; } = new List<SelectListItem>();

        // --- Data ---
        // FIX: Renamed the type to avoid conflict with Classes page
        public GradesClassViewModel SelectedClass { get; set; }

        [BindProperty]
        public List<StudentGradeViewModel> StudentGrades { get; set; } = new List<StudentGradeViewModel>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();

            // Defaults
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = SchoolYears.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            await LoadPageData();
        }

        public async Task<IActionResult> OnPostSaveGradesAsync()
        {
            if (StudentGrades != null && StudentGrades.Any())
            {
                foreach (var item in StudentGrades)
                {
                    // Find existing grade by EnrollmentId
                    var gradeRecord = await _context.Grades.FirstOrDefaultAsync(g => g.EnrollmentId == item.EnrollmentId);

                    // If it doesn't exist, create it
                    if (gradeRecord == null)
                    {
                        gradeRecord = new Grade { EnrollmentId = item.EnrollmentId };
                        _context.Grades.Add(gradeRecord);
                    }

                    gradeRecord.MidtermGrade = item.Midterm;
                    gradeRecord.FinalScore = item.Final; // Final Term Score
                    gradeRecord.FinalGrade = item.FinalGrade; // Computed Grade
                }
                await _context.SaveChangesAsync();
                TempData["Message"] = "Grades saved successfully.";
                TempData["MessageType"] = "success";
            }

            // Reload data to show updates
            await LoadDropdownsAsync();
            await LoadPageData();
            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            // 1. Dynamic School Years
            SchoolYears = new List<SelectListItem>();
            int currentYear = DateTime.Now.Year;
            if (DateTime.Now.Month < 6) currentYear--;

            for (int i = 0; i <= 7; i++)
            {
                int startYear = currentYear - i;
                string syLabel = $"{startYear}-{startYear + 1}";
                SchoolYears.Add(new SelectListItem { Value = syLabel, Text = syLabel });
            }

            // 2. Static Semesters
            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "First", Text = "First" },
                new SelectListItem { Value = "Second", Text = "Second" },
                new SelectListItem { Value = "Summer", Text = "Summer" }
            };
            await Task.CompletedTask;
        }

        private async Task LoadPageData()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            // 1. Fetch Classes for Dropdown (Filtered)
            var classesQuery = _context.Classassignments
                .Where(ca => ca.TeacherId == userId)
                .Include(ca => ca.Class).ThenInclude(c => c.Subject)
                .Include(ca => ca.Class).ThenInclude(c => c.SchoolYear)
                .Include(ca => ca.Class).ThenInclude(c => c.Semester)
                .AsQueryable();

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

            // 2. Load Data for Selected Class
            if (ClassId.HasValue && ClassId.Value > 0)
            {
                if (classes.Any(c => c.Id == ClassId.Value))
                {
                    var cls = await _context.Classes
                        .Include(c => c.Subject)
                        .Include(c => c.Classenrollments)
                            .ThenInclude(ce => ce.Student)
                        .Include(c => c.Classenrollments)
                            .ThenInclude(ce => ce.Grade)
                        .FirstOrDefaultAsync(c => c.Id == ClassId.Value);

                    if (cls != null)
                    {
                        // FIX: Using the new unique class name
                        SelectedClass = new GradesClassViewModel
                        {
                            SubjectCode = cls.Subject.SubjectCode,
                            SectionName = cls.ClassSection,
                            CourseTitle = cls.Subject.SubjectName,
                            TotalStudents = cls.Classenrollments.Count
                        };

                        StudentGrades = cls.Classenrollments.Select(ce => new StudentGradeViewModel
                        {
                            EnrollmentId = ce.Id,
                            StudentIdStr = ce.Student.StudentNumber,
                            StudentName = $"{ce.Student.LastName}, {ce.Student.FirstName}",
                            Midterm = ce.Grade?.MidtermGrade,
                            Final = ce.Grade?.FinalScore,
                            FinalGrade = ce.Grade?.FinalGrade,
                            Status = (ce.Grade?.FinalGrade <= 3.0m && ce.Grade?.FinalGrade > 0) ? "Passed" : ((ce.Grade?.FinalGrade > 3.0m) ? "Failed" : "Enrolled")
                        }).OrderBy(s => s.StudentName).ToList();
                    }
                }
                else
                {
                    ClassId = null; // Reset if invalid
                }
            }
        }
    }

    // FIX: Renamed class to avoid conflict with Classes.cshtml.cs
    public class GradesClassViewModel
    {
        public string SubjectCode { get; set; }
        public string SectionName { get; set; }
        public string CourseTitle { get; set; }
        public int TotalStudents { get; set; }
    }

    public class StudentGradeViewModel
    {
        public long EnrollmentId { get; set; }
        public string StudentIdStr { get; set; }
        public string StudentName { get; set; }
        public decimal? Midterm { get; set; }
        public decimal? Final { get; set; }
        public decimal? FinalGrade { get; set; }
        public string Status { get; set; }
    }
}