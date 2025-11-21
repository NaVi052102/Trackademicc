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
    public class DashboardModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public DashboardModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; }

        public List<SelectListItem> SchoolYears { get; set; }
        public List<SelectListItem> Semesters { get; set; }

        // --- Summary Data ---
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int AveragePerformance { get; set; } = 0;

        // --- Analytics Data ---
        public List<SubjectAnalyticsViewModel> SubjectAnalytics { get; set; } = new List<SubjectAnalyticsViewModel>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();

            // Set Defaults if empty
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = SchoolYears.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(Semester)) Semester = "First"; // Default to First

            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

           
                var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == userId);// Assuming UserId links to Teacher

            if (teacher == null) return;

            // Fetch Assigned Classes with Filtering
            var assignedClassesQuery = _context.Classassignments
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.Subject)
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.SchoolYear) // Include for filtering
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.Semester)   // Include for filtering
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.Classenrollments)
                        .ThenInclude(ce => ce.Grade)
                .Where(ca => ca.TeacherId == teacher.Id)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(SchoolYear))
            {
                assignedClassesQuery = assignedClassesQuery.Where(ca => ca.Class.SchoolYear.YearName == SchoolYear);
            }

            if (!string.IsNullOrEmpty(Semester))
            {
                assignedClassesQuery = assignedClassesQuery.Where(ca => ca.Class.Semester.SemesterName.Contains(Semester));
            }

            var assignedClasses = await assignedClassesQuery.ToListAsync();

            // Calculate Stats
            TotalClasses = assignedClasses.Count;

            TotalStudents = assignedClasses
                .SelectMany(c => c.Class.Classenrollments)
                .Select(ce => ce.StudentId)
                .Distinct()
                .Count();

            // Calculate Analytics
            foreach (var assignment in assignedClasses)
            {
                var cls = assignment.Class;
                var enrollments = cls.Classenrollments.ToList();

                int totalEnrolled = enrollments.Count;

                // Count passed/failed based on grades
                int passed = enrollments.Count(e => e.Grade != null && e.Grade.FinalGrade <= 3.0m && e.Grade.FinalGrade > 0);
                int failed = enrollments.Count(e => e.Grade != null && e.Grade.FinalGrade > 3.0m);

                int performance = totalEnrolled > 0 ? (int)((double)passed / totalEnrolled * 100) : 0;

                SubjectAnalytics.Add(new SubjectAnalyticsViewModel
                {
                    SubjectCode = cls.Subject.SubjectCode,
                    Section = cls.ClassSection,
                    PerformancePercentage = performance,
                    PassedStudents = passed,
                    FailedStudents = failed
                });
            }

            if (SubjectAnalytics.Any())
            {
                AveragePerformance = (int)SubjectAnalytics.Average(a => a.PerformancePercentage);
            }
        }

        private async Task LoadDropdownsAsync()
        {
            // 1. Generate School Years (Present down to 7 years back)
            var generatedYears = new List<SelectListItem>();
            int currentYear = DateTime.Now.Year;
            if (DateTime.Now.Month < 6) currentYear--;

            for (int i = 0; i <= 7; i++)
            {
                int startYear = currentYear - i;
                string syLabel = $"{startYear}-{startYear + 1}";
                generatedYears.Add(new SelectListItem { Value = syLabel, Text = syLabel });
            }
            SchoolYears = generatedYears;

            // 2. Update Semesters List
            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "First", Text = "First" },
                new SelectListItem { Value = "Second", Text = "Second" },
                new SelectListItem { Value = "Summer", Text = "Summer" }
            };
            await Task.CompletedTask;
        }
    }

    public class SubjectAnalyticsViewModel
    {
        public string SubjectCode { get; set; }
        public string Section { get; set; }
        public int PerformancePercentage { get; set; }
        public int PassedStudents { get; set; }
        public int FailedStudents { get; set; }
    }
}