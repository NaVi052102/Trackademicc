using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trackademic.WebApp.Pages.Admin.Students
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
        public SelectList Semesters { get; } = new SelectList(new[] { "First", "Second", "Summer" });
        public SelectList SortOptions { get; } = new SelectList(new[]
        {
            new { Value = "name", Text = "Full Name" },
            new { Value = "id", Text = "ID Number" },
            // Removed performance from sort options
            new { Value = "date", Text = "Enrollment Date" }
        }, "Value", "Text");

        // --- Data Model ---
        public IList<StudentAdminViewModel> StudentList { get; set; }

        public class StudentAdminViewModel
        {
            public int StudentID { get; set; }
            public string IDNumber { get; set; }
            public string FullName { get; set; }
            
            // NEW FIELDS for admin view
            public string Department { get; set; }
            public string ContactInfo { get; set; } // Email and Contact Number combined/simplified
            
            public System.DateTime EnrollmentDate { get; set; }
            // REMOVED: OverallPerformance
        }

        public async Task OnGetAsync()
        {
            // --- Mock Data Setup ---
            var mockData = new List<StudentAdminViewModel>
            {
                new StudentAdminViewModel
                {
                    StudentID = 101,
                    IDNumber = "S-24-001",
                    FullName = "Smith, Alice",
                    Department = "Computer Engineering", // NEW
                    ContactInfo = "alice@mail.com / 555-1234", // NEW
                    EnrollmentDate = new DateTime(2023, 09, 01),
                },
                new StudentAdminViewModel
                {
                    StudentID = 102,
                    IDNumber = "S-24-002",
                    FullName = "Johnson, Ben",
                    Department = "Electrical Engineering", // NEW
                    ContactInfo = "ben@mail.com / 555-5678", // NEW
                    EnrollmentDate = new DateTime(2023, 09, 01),
                },
                new StudentAdminViewModel
                {
                    StudentID = 103,
                    IDNumber = "S-24-003",
                    FullName = "Garcia, Clara",
                    Department = "Civil Engineering", // NEW
                    ContactInfo = "clara@mail.com / 555-9012", // NEW
                    EnrollmentDate = new DateTime(2024, 01, 15),
                }
            };

            // --- Apply Filtering (Mock Logic) ---
            IEnumerable<StudentAdminViewModel> filteredList = mockData;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredList = filteredList.Where(s =>
                    s.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.IDNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.Department.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                );
            }

            // --- Apply Sorting ---
            filteredList = SortBy switch
            {
                "id" => filteredList.OrderBy(s => s.IDNumber),
                "date" => filteredList.OrderByDescending(s => s.EnrollmentDate),
                _ => filteredList.OrderBy(s => s.FullName),
            };

            StudentList = filteredList.ToList();

            await Task.CompletedTask;
        }
    }
}