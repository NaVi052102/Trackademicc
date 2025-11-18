using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trackademic.WebApp.Pages.Admin.Teachers
{
    public class ManageModel : PageModel
    {
        // --- Input Properties for Filtering and Search ---
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name";

        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = "2024-2025";

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = "Fall";

        // Select List options
        public SelectList SchoolYears { get; } = new SelectList(new[] { "2024-2025", "2023-2024", "2022-2023" });
        public SelectList Semesters { get; } = new SelectList(new[] { "First", "Second", "Summer" });
        public SelectList SortOptions { get; } = new SelectList(new[]
        {
            new { Value = "name", Text = "Full Name" },
            new { Value = "id", Text = "Employee ID" },
            // Removed rating sort option
            new { Value = "date", Text = "Employment Date" }
        }, "Value", "Text");

        // --- Data Model ---
        public IList<TeacherViewModel> TeacherList { get; set; } = new List<TeacherViewModel>();

        public class TeacherViewModel
        {
            public int TeacherID { get; set; }
            public string EmployeeID { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            
            // NEW FIELDS
            public string Department { get; set; } = string.Empty; 
            public string ContactInfo { get; set; } = string.Empty; 
            
            public System.DateTime EmploymentDate { get; set; }
            // REMOVED: ClassesTaught and AverageRating
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
                    Department = "Computer Engineering",
                    ContactInfo = "maria@school.edu / 555-1234",
                    EmploymentDate = new DateTime(2022, 08, 15),
                },
                new TeacherViewModel
                {
                    TeacherID = 502,
                    EmployeeID = "T-24-001",
                    FullName = "Reyes, Jose",
                    Department = "Electrical Engineering",
                    ContactInfo = "jose@school.edu / 555-5678",
                    EmploymentDate = new DateTime(2024, 01, 05),
                },
                new TeacherViewModel
                {
                    TeacherID = 503,
                    EmployeeID = "T-23-055",
                    FullName = "Santos, Leah",
                    Department = "General Education",
                    ContactInfo = "leah@school.edu / 555-9012",
                    EmploymentDate = new DateTime(2023, 09, 01),
                }
            };

            // --- Apply Filtering (Mock Logic) ---
            IEnumerable<TeacherViewModel> filteredList = mockData;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredList = filteredList.Where(t =>
                    t.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.EmployeeID.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Department.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
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