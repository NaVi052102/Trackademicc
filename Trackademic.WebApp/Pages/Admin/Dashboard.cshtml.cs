using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public DashboardModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string? SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Semester { get; set; }

        public List<SelectListItem> SchoolYears { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Semesters { get; set; } = new List<SelectListItem>();

        // --- Summary Data (Info Cards) ---
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalSubjects { get; set; }

        // --- Departmental Analytics Data ---
        public List<DepartmentAnalyticsViewModel> DepartmentalPerformance { get; set; } = new List<DepartmentAnalyticsViewModel>();

        public async Task OnGetAsync()
        {
            // 1. Populate Filter Dropdowns from DB
            SchoolYears = await _context.Schoolyears
                .OrderByDescending(y => y.YearName)
                .Select(y => new SelectListItem { Value = y.YearName, Text = y.YearName })
                .ToListAsync();

            Semesters = await _context.Semesters
                .Select(s => s.SemesterName)
                .Distinct()
                .Select(n => new SelectListItem { Value = n, Text = n })
                .ToListAsync();

            // Set Defaults if empty
            if (string.IsNullOrEmpty(SchoolYear) && SchoolYears.Any())
                SchoolYear = SchoolYears.First().Value;

            if (string.IsNullOrEmpty(Semester) && Semesters.Any())
                Semester = Semesters.First().Value;

            // 2. Load Summary Counts (Global Counts)
            TotalStudents = await _context.Students.CountAsync();
            TotalTeachers = await _context.Teachers.CountAsync();
            TotalDepartments = await _context.Departments.CountAsync();
            TotalSubjects = await _context.Subjects.CountAsync();

            // 3. Load Analytics based on Filters
            await LoadDepartmentalAnalytics();
        }

        private async Task LoadDepartmentalAnalytics()
        {
            if (string.IsNullOrEmpty(SchoolYear) || string.IsNullOrEmpty(Semester)) return;

            // Get all departments first
            var departments = await _context.Departments.ToListAsync();

            foreach (var dept in departments)
            {
                // Find classes for this department, year, and semester
                // We start from Classes -> Subject -> Department
                var classesQuery = _context.Classes
                    .Include(c => c.Classenrollments)
                        .ThenInclude(ce => ce.Grade)
                    .Where(c => c.Subject.DepartmentId == dept.Id
                             && c.SchoolYear.YearName == SchoolYear
                             && c.Semester.SemesterName == Semester);

                var classes = await classesQuery.ToListAsync();

                // 1. Classes Offered
                int classesOffered = classes.Count;

                // Collect all grades from these classes to calculate averages
                // We filter out enrollments with no grades (FinalScore is null)
                var allGrades = classes
                    .SelectMany(c => c.Classenrollments)
                    .Select(ce => ce.Grade)
                    .Where(g => g != null && g.FinalScore != null)
                    .ToList();

                double avgperformance = 0;
                int passingRate = 0;

                if (allGrades.Any())
                {
                    // 2. Average Performance (Using FinalScore 0-100)
                    avgperformance = (double)allGrades.Average(g => g!.FinalScore!.Value);

                    // 3. Passing Rate
                    // Assuming Passing Score is >= 75. Adjust this logic if using 3.0/5.0 system.
                    int passedCount = allGrades.Count(g => g!.FinalScore!.Value >= 75);
                    passingRate = (int)((double)passedCount / allGrades.Count * 100);
                }

                DepartmentalPerformance.Add(new DepartmentAnalyticsViewModel
                {
                    DepartmentName = dept.DeptName,
                    AveragePerformance = (int)avgperformance,
                    PassingRate = passingRate,
                    ClassesOffered = classesOffered
                });
            }
        }
    }

    // ViewModel for Departmental Performance Analytics
    public class DepartmentAnalyticsViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int AveragePerformance { get; set; }
        public int PassingRate { get; set; }
        public int ClassesOffered { get; set; }
    }
}