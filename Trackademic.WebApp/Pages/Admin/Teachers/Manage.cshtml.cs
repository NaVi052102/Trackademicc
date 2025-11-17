using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trackademic.Pages.Admin.Teachers
{
    public class ManageModel : PageModel
    {
        // --- Input Properties for Filtering and Search ---
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name"; // Default sort by name

        // Using School Year and Semester as general filter criteria, although less common for teachers
        // For a more accurate teacher list, you might use 'Department' or 'Status' instead.
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = "2024-2025";

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = "Fall";

        // Select List options
        public SelectList SchoolYears { get; } = new SelectList(new[] { "2024-2025", "2023-2024", "2022-2023" });
        public SelectList Semesters { get; } = new SelectList(new[] { "Fall", "Spring", "Summer" });
        public SelectList SortOptions { get; } = new SelectList(new[]
        {
            new { Value = "name", Text = "Full Name" },
            new { Value = "id", Text = "Employee ID" },
            new { Value = "date", Text = "Employment Date" }
        }, "Value", "Text");

        // --- Data Model ---
        public IList<TeacherViewModel> TeacherList { get; set; }

        public class TeacherViewModel
        {
            public int TeacherID { get; set; }
            public string EmployeeID { get; set; } // Renamed from IDNumber
            public string FullName { get; set; }
            public string Email { get; set; }
            public System.DateTime EmploymentDate { get; set; } // Renamed from EnrollmentDate
            public int ClassesTaught { get; set; } // Example metric
            public double AverageRating { get; set; } // Example metric
        }

        public async Task OnGetAsync()
        {
            // --- Mock Data Setup (Teacher Data) ---
            var mockData = new List<TeacherViewModel>
            {
                new TeacherViewModel
                {
                    TeacherID = 501,
                    EmployeeID = "T-22-010",
                    FullName = "Dela Cruz, Maria",
                    Email = "maria@school.edu",
                    EmploymentDate = new DateTime(2022, 08, 15),
                    ClassesTaught = 3,
                    AverageRating = 4.7
                },
                new TeacherViewModel
                {
                    TeacherID = 502,
                    EmployeeID = "T-24-001",
                    FullName = "Reyes, Jose",
                    Email = "jose@school.edu",
                    EmploymentDate = new DateTime(2024, 01, 05),
                    ClassesTaught = 2,
                    AverageRating = 4.1
                },
                new TeacherViewModel
                {
                    TeacherID = 503,
                    EmployeeID = "T-23-055",
                    FullName = "Santos, Leah",
                    Email = "leah@school.edu",
                    EmploymentDate = new DateTime(2023, 09, 01),
                    ClassesTaught = 4,
                    AverageRating = 4.9
                }
            };

            // --- Apply Filtering (Mock Logic) ---
            IEnumerable<TeacherViewModel> filteredList = mockData;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredList = filteredList.Where(t =>
                    t.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.EmployeeID.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                );
            }

            // --- Apply Sorting ---
            filteredList = SortBy switch
            {
                "id" => filteredList.OrderBy(t => t.EmployeeID),
                "date" => filteredList.OrderByDescending(t => t.EmploymentDate),
                _ => filteredList.OrderBy(t => t.FullName),
            };

            TeacherList = filteredList.ToList();

            await Task.CompletedTask;
        }
    }
}