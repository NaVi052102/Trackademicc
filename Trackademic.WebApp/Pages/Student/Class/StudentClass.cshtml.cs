using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trackademic.Data.Data;

namespace Trackademic.WebApp.Pages.Student
{
    public class StudentClassModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public StudentClassModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty; // Added Search Term

        // --- Dropdowns ---
        public List<SelectListItem> SchoolYears { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Semesters { get; set; } = new List<SelectListItem>();

        // --- Data ---
        public List<ClassCardViewModel> Classes { get; set; } = new List<ClassCardViewModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Get Logged-in Student ID
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long studentId))
            {
                return RedirectToPage("/Account/Login");
            }

            // 2. POPULATE FILTERS
            var generatedYears = new List<SelectListItem>();
            int currentYear = DateTime.Now.Year;
            if (DateTime.Now.Month < 6) currentYear--;

            for (int i = 0; i <= 7; i++)
            {
                int startYear = currentYear - i;
                string syLabel = $"{startYear}-{startYear + 1}";
                generatedYears.Add(new SelectListItem { Value = syLabel, Text = syLabel });
            }
            SchoolYears = generatedYears;

            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "First", Text = "First" },
                new SelectListItem { Value = "Second", Text = "Second" },
                new SelectListItem { Value = "Summer", Text = "Summer" },
            };

            // 3. SET DEFAULTS
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = SchoolYears.FirstOrDefault()?.Value ?? "";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            // 4. FETCH & FILTER
            var query = _context.Classenrollments
                .Where(ce => ce.StudentId == studentId)
                .Include(ce => ce.Class)
                    .ThenInclude(c => c.Subject)
                .Include(ce => ce.Class)
                    .ThenInclude(c => c.Classassignments)
                        .ThenInclude(ca => ca.Teacher)
                .Include(ce => ce.Class)
                    .ThenInclude(c => c.Classenrollments)
                .Include(ce => ce.Class)
                    .ThenInclude(c => c.SchoolYear)
                .Include(ce => ce.Class)
                    .ThenInclude(c => c.Semester)
                .AsQueryable();

            // --- APPLY FILTERS ---

            // Filter by School Year
            if (!string.IsNullOrEmpty(SchoolYear))
            {
                query = query.Where(ce => ce.Class.SchoolYear.YearName == SchoolYear);
            }

            // Filter by Semester
            if (!string.IsNullOrEmpty(Semester))
            {
                query = query.Where(ce => ce.Class.Semester.SemesterName.Contains(Semester));
            }

            // Filter by Search Term (Subject Name)
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string term = SearchTerm.ToLower();
                query = query.Where(ce => ce.Class.Subject.SubjectName.ToLower().Contains(term));
            }

            var classEntities = await query.ToListAsync();

            // 5. Map to ViewModel
            Classes = classEntities.Select(ce => new ClassCardViewModel
            {
                ClassId = ce.ClassId,
                Title = ce.Class.Subject.SubjectName ?? "Unknown Subject",
                StudentCount = ce.Class.Classenrollments.Count,
                // Logic for Instructor Name
                InstructorName = ce.Class.Classassignments.Any() && ce.Class.Classassignments.First().Teacher != null
                    ? $"{ce.Class.Classassignments.First().Teacher.FirstName} {ce.Class.Classassignments.First().Teacher.LastName}"
                    : "TBA"
            }).ToList();

            return Page();
        }

        // --- AJAX Handler for Roster ---
        public async Task<JsonResult> OnGetClassRosterAsync(long classId)
        {
            var students = await _context.Classenrollments
                .Where(ce => ce.ClassId == classId)
                .Include(ce => ce.Student)
                .OrderBy(ce => ce.Student.LastName)
                .Select(ce => new
                {
                    fullName = $"{ce.Student.FirstName} {ce.Student.LastName}",
                    studentNumber = ce.Student.StudentNumber
                })
                .ToListAsync();

            return new JsonResult(students);
        }
    }

    public class ClassCardViewModel
    {
        public long ClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string InstructorName { get; set; } = string.Empty;

        // Helper to get Initials (e.g. "Data Structures" -> "DS")
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title)) return "??";
                var words = Title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                    return $"{words[0][0]}{words[1][0]}".ToUpper();
                else
                    return words[0].Length > 1 ? words[0].Substring(0, 2).ToUpper() : words[0].ToUpper();
            }
        }
    }
}