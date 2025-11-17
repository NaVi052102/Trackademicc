using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        // --- Filter Properties (Kept for period-based reporting) ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; }

        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2930", Text = "2930" },
            new SelectListItem { Value = "2829", Text = "2829" },
            new SelectListItem { Value = "2728", Text = "2728" },
            new SelectListItem { Value = "2627", Text = "2627" },
            new SelectListItem { Value = "2526", Text = "2526" },
            new SelectListItem { Value = "2425", Text = "2425" },
            new SelectListItem { Value = "2324", Text = "2324" },
            new SelectListItem { Value = "2223", Text = "2223" },
        };

        public List<SelectListItem> Semesters { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "First", Text = "First" },
            new SelectListItem { Value = "Second", Text = "Second" },
            new SelectListItem { Value = "Summer", Text = "Summer" },
        };

        // --- System-Wide Summary Data ---
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalSubjects { get; set; }
        // OverallGradeAverage property removed as requested

        // Departmental Performance Analytics
        public List<DepartmentalPerformanceItem> DepartmentalPerformance { get; set; } = new List<DepartmentalPerformanceItem>();

        // RecentActivities list removed as requested


        public void OnGet()
        {
            // Set defaults for the filters
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            // 1. Fetch System-Wide Summary Data (Dummy Data)
            TotalStudents = 1200;
            TotalTeachers = 45;
            TotalDepartments = 8;
            TotalSubjects = 150;
            // OverallGradeAverage assignment removed

            // 2. Load Departmental Performance Data
            LoadDepartmentalPerformance();

            // 3. Load Recent Activities (System Snapshot) - REMOVED
            // LoadRecentActivities();
        }

        private void LoadDepartmentalPerformance()
        {
            // Simulate fetching performance data aggregated by department
            DepartmentalPerformance.Add(new DepartmentalPerformanceItem
            {
                DepartmentName = "Computer Engineering",
                AveragePerformance = 85,
                ClassesOffered = 22,
                PassingRate = 92 // 92% of students in this dept passed their classes
            });
            DepartmentalPerformance.Add(new DepartmentalPerformanceItem
            {
                DepartmentName = "Electronics Engineering",
                AveragePerformance = 72,
                ClassesOffered = 18,
                PassingRate = 78
            });
            DepartmentalPerformance.Add(new DepartmentalPerformanceItem
            {
                DepartmentName = "General Education",
                AveragePerformance = 91,
                ClassesOffered = 45,
                PassingRate = 98
            });
        }

        // private void LoadRecentActivities() { ... } // Method removed
    }

    // ViewModel for Departmental Performance Analytics (Retained)
    public class DepartmentalPerformanceItem
    {
        public string DepartmentName { get; set; }
        public int AveragePerformance { get; set; } // Average student grade in the department
        public int ClassesOffered { get; set; }
        public int PassingRate { get; set; } // % of students who passed their classes
    }

    // ViewModel for Recent System Activity (Removed as requested, but keeping the class definition here 
    // for completeness, though it is no longer used in the DashboardModel)
    public class RecentActivityItem
    {
        public string Description { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; } // e.g., "Approval", "Alert", "System"
    }
}