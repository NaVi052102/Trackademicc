using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trackademic.Pages.Admin.Students
{
    public class ManageModel : PageModel
    {
        // --- Input Properties for Filtering and Search ---
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name"; // Default sort by name

        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = "2024-2025"; // Default Year

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = "Fall"; // Default Semester

        // Select List options
        public SelectList SchoolYears { get; } = new SelectList(new[] { "2024-2025", "2023-2024", "2022-2023" });
        public SelectList Semesters { get; } = new SelectList(new[] { "Fall", "Spring", "Summer" });
        public SelectList SortOptions { get; } = new SelectList(new[]
        {
            new { Value = "name", Text = "Full Name" },
            new { Value = "id", Text = "ID Number" },
            new { Value = "performance", Text = "Performance" },
            new { Value = "date", Text = "Enrollment Date" }
        }, "Value", "Text");

        // --- Data Model ---
        public IList<StudentPerformanceViewModel> StudentList { get; set; }

        public class StudentPerformanceViewModel
        {
            public int StudentID { get; set; }
            public string IDNumber { get; set; } // New field for ID number
            public string FullName { get; set; }
            public string Email { get; set; }
            public System.DateTime EnrollmentDate { get; set; }
            public double OverallPerformance { get; set; }
        }

        public async Task OnGetAsync()
        {
            // --- Mock Data Setup ---
            var mockData = new List<StudentPerformanceViewModel>
            {
                new StudentPerformanceViewModel
                {
                    StudentID = 101,
                    IDNumber = "S-24-001", // Example ID
                    FullName = "Smith, Alice",
                    Email = "alice@school.edu",
                    EnrollmentDate = new DateTime(2023, 09, 01),
                    OverallPerformance = 92.5
                },
                new StudentPerformanceViewModel
                {
                    StudentID = 102,
                    IDNumber = "S-24-002",
                    FullName = "Johnson, Ben",
                    Email = "ben@school.edu",
                    EnrollmentDate = new DateTime(2023, 09, 01),
                    OverallPerformance = 78.1
                },
                new StudentPerformanceViewModel
                {
                    StudentID = 103,
                    IDNumber = "S-24-003",
                    FullName = "Garcia, Clara",
                    Email = "clara@school.edu",
                    EnrollmentDate = new DateTime(2024, 01, 15),
                    OverallPerformance = 65.0
                }
            };

            // --- Apply Filtering (Mock Logic) ---
            IEnumerable<StudentPerformanceViewModel> filteredList = mockData;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredList = filteredList.Where(s =>
                    s.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.IDNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                );
            }
            // Note: Since this is mock data, we skip filtering by SchoolYear/Semester

            // --- Apply Sorting ---
            filteredList = SortBy switch
            {
                "id" => filteredList.OrderBy(s => s.IDNumber),
                "performance" => filteredList.OrderByDescending(s => s.OverallPerformance),
                "date" => filteredList.OrderByDescending(s => s.EnrollmentDate),
                _ => filteredList.OrderBy(s => s.FullName),
            };

            StudentList = filteredList.ToList();

            await Task.CompletedTask;
        }
    }
}