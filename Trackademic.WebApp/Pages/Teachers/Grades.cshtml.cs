using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trackademic.Data.Data;   // TrackademicDbContext
using Trackademic.Data.Models; // Data Models

namespace Trackademic.WebApp.Pages.Teachers
{
    [Authorize(Roles = "Teacher")]
    public class GradesModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public GradesModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Properties ---
        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; } = "2024-2025";
        [BindProperty(SupportsGet = true)] public string Semester { get; set; } = "1st Semester";
        [BindProperty(SupportsGet = true)] public long? ClassId { get; set; }

        // The list that binds to the HTML Form
        [BindProperty] public List<GradeEntryViewModel> GradesList { get; set; } = new List<GradeEntryViewModel>();

        // Dropdowns & Headers
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();
        public string SubjectDescription { get; set; } = "Select a class";

        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem> { new SelectListItem { Value = "2024-2025", Text = "2024-2025" } };
        public List<SelectListItem> Semesters { get; } = new List<SelectListItem> { new SelectListItem { Value = "1st Semester", Text = "1st Semester" } };

        public async Task OnGetAsync()
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            // 1. Load Classes Assigned to Teacher
            var teacherClasses = await _context.Classassignments
                .Where(ca => ca.TeacherId == userId)
                .Include(ca => ca.Class).ThenInclude(c => c.Subject)
                .Select(ca => new
                {
                    ca.Class.Id,
                    Text = $"{ca.Class.Subject.SubjectCode} - {ca.Class.ClassSection}"
                })
                .ToListAsync();

            AvailableClasses = teacherClasses.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Text }).ToList();

            // 2. If Class Selected, Load Grades
            if (ClassId.HasValue && ClassId.Value > 0)
            {
                await LoadGradesData(ClassId.Value);
            }
        }

        private async Task LoadGradesData(long classId)
        {
            // Header Info
            var cls = await _context.Classes.Include(c => c.Subject).FirstOrDefaultAsync(c => c.Id == classId);
            if (cls != null) SubjectDescription = $"{cls.Subject.SubjectCode} - {cls.Subject.SubjectName}";

            // Fetch Grades
            var dbGrades = await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(ce => ce.Student)
                .Where(g => g.Enrollment.ClassId == classId)
                .ToListAsync();

            // Map to ViewModel
            GradesList = dbGrades.Select(g => new GradeEntryViewModel
            {
                GradeId = g.Id,
                StudentName = $"{g.Enrollment.Student.LastName}, {g.Enrollment.Student.FirstName}",
                StudentNumber = g.Enrollment.Student.StudentNumber,
                Midterm = g.MidtermGrade,
                Final = g.FinalGrade,
                FinalScore = g.FinalScore
            }).OrderBy(x => x.StudentName).ToList();
        }

        public async Task<IActionResult> OnPostUpdateAllAsync()
        {
            if (GradesList == null || !GradesList.Any()) return Page();

            foreach (var item in GradesList)
            {
                var grade = await _context.Grades.FindAsync(item.GradeId);
                if (grade != null)
                {
                    // Update DB values from Form
                    grade.MidtermGrade = item.Midterm;
                    grade.FinalGrade = item.Final;

                    // Auto-Calculate Final Score (Average)
                    if (grade.MidtermGrade.HasValue && grade.FinalGrade.HasValue)
                    {
                        grade.FinalScore = (grade.MidtermGrade.Value + grade.FinalGrade.Value) / 2.0m;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Grades successfully saved!";
            TempData["MessageType"] = "success";

            return RedirectToPage(new { SchoolYear, Semester, ClassId });
        }
    }

    public class GradeEntryViewModel
    {
        public long GradeId { get; set; }
        public string StudentNumber { get; set; }
        public string StudentName { get; set; }
        public decimal? Midterm { get; set; }
        public decimal? Final { get; set; }
        public decimal? FinalScore { get; set; }
    }
}