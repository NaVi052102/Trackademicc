using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin.Teachers
{
    public class ManageModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public ManageModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Input Properties for Filtering and Search ---
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name";

        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; }

        // Select List options
        public SelectList SchoolYears { get; set; }
        public SelectList Semesters { get; set; }
        public SelectList SortOptions { get; } = new SelectList(new[]
        {
            new { Value = "name", Text = "Full Name" },
            new { Value = "id", Text = "Employee ID" },
            new { Value = "date", Text = "Date of Birth" } // Mapped to DOB since EmploymentDate doesn't exist
        }, "Value", "Text");

        // --- Data Model ---
        public IList<TeacherViewModel> TeacherList { get; set; } = new List<TeacherViewModel>();

        public async Task OnGetAsync()
        {
            // 1. Populate Dropdowns
            var years = await _context.Schoolyears.OrderByDescending(y => y.YearName).Select(y => y.YearName).ToListAsync();
            var sems = await _context.Semesters.Select(s => s.SemesterName).Distinct().ToListAsync();

            SchoolYears = new SelectList(years);
            Semesters = new SelectList(sems);

            // 2. Start Query
            // Include Department for display
            var query = _context.Teachers
                .Include(t => t.Department)
                .AsQueryable();

            // 3. Apply Search
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string term = SearchTerm.ToLower();
                query = query.Where(t =>
                    t.FirstName.ToLower().Contains(term) ||
                    t.LastName.ToLower().Contains(term) ||
                    t.TeacherId.ToLower().Contains(term) ||
                    t.Department.DeptName.ToLower().Contains(term));
            }

            // 4. Apply Term Filter (Optional)
            // "Show teachers who have at least one class assigned in this Year/Sem"
            if (!string.IsNullOrEmpty(SchoolYear) && !string.IsNullOrEmpty(Semester))
            {
                // We filter teachers where ANY of their ClassAssignments -> Class -> SchoolYear matches
                query = query.Where(t => t.Classassignments.Any(ca =>
                    ca.Class.SchoolYear.YearName == SchoolYear &&
                    ca.Class.Semester.SemesterName == Semester
                ));
            }

            // 5. Apply Sorting
            switch (SortBy)
            {
                case "id":
                    query = query.OrderBy(t => t.TeacherId);
                    break;
                case "date":
                    query = query.OrderByDescending(t => t.DateOfBirth);
                    break;
                default: // "name"
                    query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
                    break;
            }

            // 6. Execute & Map
            var teachers = await query.ToListAsync();

            TeacherList = teachers.Select(t => new TeacherViewModel
            {
                TeacherID = t.Id,
                EmployeeID = t.TeacherId,
                FullName = $"{t.LastName}, {t.FirstName}",
                Department = t.Department?.DeptName ?? "N/A",
                ContactInfo = t.Email ?? t.ContactNumber ?? "N/A",

                // NOTE: Database doesn't have EmploymentDate, mapping DateOfBirth for now.
                // You can change this to DateTime.Now if you prefer a placeholder.
                EmploymentDate = t.DateOfBirth.HasValue
                    ? t.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue
            }).ToList();
        }

        public class TeacherViewModel
        {
            public long TeacherID { get; set; } // Changed int to long to match DB
            public string EmployeeID { get; set; }
            public string FullName { get; set; }

            public string Department { get; set; }
            public string ContactInfo { get; set; }

            public DateTime EmploymentDate { get; set; }
        }
    }
}