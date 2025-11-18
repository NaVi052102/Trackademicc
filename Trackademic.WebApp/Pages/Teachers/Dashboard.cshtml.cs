using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Teachers
{
    public class DashboardModel : PageModel
    {
        // --- Filter Properties ---
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

        // --- Summary Data ---
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int AveragePerformance { get; set; } = 0;

        // NEW: Detailed Subject Analytics
        public List<SubjectAnalyticsViewModel> SubjectAnalytics { get; set; } = new List<SubjectAnalyticsViewModel>();
        
        // REMOVED: TodaysSchedule List
        // REMOVED: ScheduleSnapshotItem class


        public void OnGet()
        {
            // Set defaults for the filters
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";
            
            // 1. Fetch Summary Data (Dummy Data)
            TotalClasses = 3;
            TotalStudents = 125;
            
            // 2. Fetch Analytics Data (Dummy Data)
            LoadSubjectAnalytics();
            
            // Calculate overall performance average
            if (SubjectAnalytics.Any())
            {
                // Calculate average performance across all subjects
                AveragePerformance = (int)SubjectAnalytics.Average(a => a.PerformancePercentage);
            }

            // REMOVED: Logic for fetching Today's Schedule
        }

        private void LoadSubjectAnalytics()
        {
            // Simulate fetching subject performance data for the selected period
            SubjectAnalytics.Add(new SubjectAnalyticsViewModel 
            { 
                SubjectCode = "CPE461", 
                Section = "H2-4R4", 
                PerformancePercentage = 88,
                PassedStudents = 32,
                FailedStudents = 3
            });
            SubjectAnalytics.Add(new SubjectAnalyticsViewModel 
            { 
                SubjectCode = "CPE461", 
                Section = "H3-4R4", 
                PerformancePercentage = 76,
                PassedStudents = 35,
                FailedStudents = 5
            });
            SubjectAnalytics.Add(new SubjectAnalyticsViewModel 
            { 
                SubjectCode = "ES032", 
                Section = "K3-4R4", 
                PerformancePercentage = 55,
                PassedStudents = 15,
                FailedStudents = 35
            });
        }
        
        // REMOVED: GetSimulatedClassData method
    }

    // Updated: ViewModel for Subject Performance Analytics
    public class SubjectAnalyticsViewModel
    {
        public string SubjectCode { get; set; }
        public string Section { get; set; }
        public int PerformancePercentage { get; set; }
        public int PassedStudents { get; set; }
        public int FailedStudents { get; set; }
    }
}