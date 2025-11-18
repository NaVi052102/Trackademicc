using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Admin
{
    // Ensure the namespace matches your file path, e.g., Trackademic.Pages.Admin
    public class DashboardModel : PageModel
    {
        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; }

        // Dropdown Lists (Static for now)
        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2930", Text = "2930" },
            new SelectListItem { Value = "2829", Text = "2829" },
            new SelectListItem { Value = "2425", Text = "2425" }
        };

        public List<SelectListItem> Semesters { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "First", Text = "First" },
            new SelectListItem { Value = "Second", Text = "Second" },
            new SelectListItem { Value = "Summer", Text = "Summer" }
        };

        // --- Summary Data (Info Cards) ---
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalSubjects { get; set; }

        // --- Departmental Analytics Data ---
        public List<DepartmentAnalyticsViewModel> DepartmentalPerformance { get; set; } = new List<DepartmentAnalyticsViewModel>();


        public void OnGet()
        {
            // Set defaults if no filters are selected
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";
            
            // 1. Load Summary Data (Simulated)
            TotalStudents = 1250;
            TotalTeachers = 45;
            TotalDepartments = 8;
            TotalSubjects = 150;
            
            // 2. Load Departmental Analytics (Simulated)
            LoadDepartmentalAnalytics();
        }

        private void LoadDepartmentalAnalytics()
        {
            // Simulated data for the Departmental Performance card
            DepartmentalPerformance.Add(new DepartmentAnalyticsViewModel 
            { 
                DepartmentName = "Computer Engineering", 
                AveragePerformance = 82,
                PassingRate = 91,
                ClassesOffered = 15
            });
            DepartmentalPerformance.Add(new DepartmentAnalyticsViewModel 
            { 
                DepartmentName = "Civil Engineering", 
                AveragePerformance = 71,
                PassingRate = 78,
                ClassesOffered = 12
            });
            DepartmentalPerformance.Add(new DepartmentAnalyticsViewModel 
            { 
                DepartmentName = "Electrical Engineering", 
                AveragePerformance = 65,
                PassingRate = 60,
                ClassesOffered = 10
            });
            DepartmentalPerformance.Add(new DepartmentAnalyticsViewModel 
            { 
                DepartmentName = "General Education", 
                AveragePerformance = 88,
                PassingRate = 95,
                ClassesOffered = 25
            });
        }
    }

    // ViewModel for Departmental Performance Analytics
    public class DepartmentAnalyticsViewModel
    {
        public string DepartmentName { get; set; }
        public int AveragePerformance { get; set; } // Overall student average score in the department's classes
        public int PassingRate { get; set; }        // Percentage of students passing
        public int ClassesOffered { get; set; }     // Total classes offered in the period
    }
}