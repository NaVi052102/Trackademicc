using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin.Students
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
            new { Value = "id", Text = "ID Number" },
            new { Value = "date", Text = "Enrollment Date" }
        }, "Value", "Text");

        // --- Data Model ---
        public IList<StudentAdminViewModel> StudentList { get; set; } = new List<StudentAdminViewModel>();

        public async Task OnGetAsync()
        {
            // 1. Populate Dropdowns from Database
            var years = await _context.Schoolyears
                .OrderByDescending(y => y.YearName)
                .Select(y => y.YearName)
                .ToListAsync();

            var sems = await _context.Semesters
                .Select(s => s.SemesterName)
                .Distinct()
                .ToListAsync();

            SchoolYears = new SelectList(years);
            Semesters = new SelectList(sems);

            // Set defaults if not provided
            if (string.IsNullOrEmpty(SchoolYear) && years.Any()) SchoolYear = years.First();
            if (string.IsNullOrEmpty(Semester) && sems.Any()) Semester = sems.First();

            // 2. Start Query
            // We include ClassEnrollments to check active status and get enrollment date
            var query = _context.Students
                .Include(s => s.Classenrollments)
                    .ThenInclude(ce => ce.Class)
                        .ThenInclude(c => c.SchoolYear)
                .Include(s => s.Classenrollments)
                    .ThenInclude(ce => ce.Class)
                        .ThenInclude(c => c.Semester)
                .AsQueryable();

            // 3. Apply Search
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string term = SearchTerm.ToLower();
                query = query.Where(s =>
                    s.FirstName.ToLower().Contains(term) ||
                    s.LastName.ToLower().Contains(term) ||
                    s.StudentNumber.ToLower().Contains(term) ||
                    (s.CourseProgram != null && s.CourseProgram.ToLower().Contains(term))
                );
            }

            // 4. Apply Term Filter (Active Students Only)
            // "Show students who have at least one class enrolled in this specific Year/Sem"
            if (!string.IsNullOrEmpty(SchoolYear) && !string.IsNullOrEmpty(Semester))
            {
                query = query.Where(s => s.Classenrollments.Any(ce =>
                    ce.Class.SchoolYear.YearName == SchoolYear &&
                    ce.Class.Semester.SemesterName == Semester
                ));
            }

            // 5. Apply Sorting
            switch (SortBy)
            {
                case "id":
                    query = query.OrderBy(s => s.StudentNumber);
                    break;
                case "date":
                    // Sort by the earliest enrollment date found for the student
                    query = query.OrderByDescending(s => s.Classenrollments.Min(ce => ce.EnrollmentDate));
                    break;
                default: // "name"
                    query = query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);
                    break;
            }

            // 6. Execute Query
            var students = await query.ToListAsync();

            // 7. Map to ViewModel
            StudentList = students.Select(s => new StudentAdminViewModel
            {
                StudentID = s.Id,
                IDNumber = s.StudentNumber,
                FullName = $"{s.LastName}, {s.FirstName}",

                // Using 'CourseProgram' from database
                Department = s.CourseProgram ?? "N/A",

                ContactInfo = FormatContactInfo(s.Email, s.ContactNumber),

                // Determine "Enrollment Date" (Earliest class joined, or default min value if none)
                EnrollmentDate = s.Classenrollments.Any()
                    ? s.Classenrollments.Min(ce => ce.EnrollmentDate).ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue
            }).ToList();
        }

        private string FormatContactInfo(string? email, string? phone)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(email)) parts.Add(email);
            if (!string.IsNullOrEmpty(phone)) parts.Add(phone);
            return parts.Any() ? string.Join(" / ", parts) : "N/A";
        }

        public class StudentAdminViewModel
        {
            public long StudentID { get; set; }
            public string IDNumber { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string ContactInfo { get; set; }
            public DateTime EnrollmentDate { get; set; }
        }
    }
}