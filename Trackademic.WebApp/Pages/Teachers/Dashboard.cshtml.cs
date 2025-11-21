using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trackademic.Data.Data;   // FIX: Points to TrackademicDbContext
using Trackademic.Data.Models; // FIX: Points to Data Models

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        private readonly TrackademicDbContext _context; // FIX: Context type

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

            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            // Get Teacher Profile
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == userId);

            if (teacher == null) return;

            // Fetch Assigned Classes
            // FIX: Using 'Classassignments' (lowercase 'a') to match TrackademicDbContext
            var assignedClassesQuery = _context.Classassignments
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.Subject)
                .Include(ca => ca.Class)
                    .ThenInclude(c => c.Classenrollments) // FIX: Lowercase 'e'
                        .ThenInclude(ce => ce.Grade)
                .Where(ca => ca.TeacherId == teacher.Id); // Note: Check if Model uses TeacherId (long) or Teacher string ID

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
                // FIX: Check FinalScore (or FinalGrade depending on your DB column)
                int passed = enrollments.Count(e => e.Grade != null && e.Grade.FinalScore <= 3.0m && e.Grade.FinalScore > 0);
                int failed = enrollments.Count(e => e.Grade != null && e.Grade.FinalScore > 3.0m);

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
            SchoolYears = new List<SelectListItem>
            {
                new SelectListItem { Value = "2024-2025", Text = "2024-2025" },
                new SelectListItem { Value = "2025-2026", Text = "2025-2026" }
            };

            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "1st Semester", Text = "1st Semester" },
                new SelectListItem { Value = "2nd Semester", Text = "2nd Semester" }
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