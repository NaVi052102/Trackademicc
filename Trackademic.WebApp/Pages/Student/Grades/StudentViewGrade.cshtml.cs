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

namespace Trackademic.WebApp.Pages.Student.Grades
{
    public class StudentViewGradeModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public StudentViewGradeModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = string.Empty;

        public List<SelectListItem> SchoolYears { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Semesters { get; set; } = new List<SelectListItem>();

        // --- Summary Data ---
        public decimal SemesterGPA { get; set; } = 0.0m;
        public int TotalUnits { get; set; } = 0;
        public int SubjectsPassed { get; set; } = 0;
        public int SubjectsFailed { get; set; } = 0;

        // --- Table Data ---
        public List<GradeViewModel> GradeList { get; set; } = new List<GradeViewModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Authenticate
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long studentId))
            {
                return RedirectToPage("/Account/Login");
            }

            // 2. Populate Filters
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

            // 3. Set Defaults
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = SchoolYears.FirstOrDefault()?.Value ?? "2024-2025";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            // 4. Fetch Data
            var enrollments = await _context.Classenrollments
                .Where(ce => ce.StudentId == studentId)
                .Where(ce => ce.Class.SchoolYear.YearName == SchoolYear)
                .Where(ce => ce.Class.Semester.SemesterName.Contains(Semester))
                .Include(ce => ce.Class).ThenInclude(c => c.Subject)
                .Include(ce => ce.Class).ThenInclude(c => c.Classassignments).ThenInclude(ca => ca.Teacher)
                .Include(ce => ce.Grade)
                .ToListAsync();

            // 5. Process Data
            decimal totalGradePoints = 0;
            int totalUnitsForGpa = 0;

            foreach (var enrollment in enrollments)
            {
                // Determine Teacher Name
                string teacherName = enrollment.Class.Classassignments.Any() && enrollment.Class.Classassignments.First().Teacher != null
                    ? $"{enrollment.Class.Classassignments.First().Teacher.FirstName} {enrollment.Class.Classassignments.First().Teacher.LastName}"
                    : "TBA";

                // Get Units
                int units = enrollment.Class.Subject.CreditUnits ?? 3;

                // --- GRADE MAPPING ---
                string midtermDisplay = "N/A";
                string finalTermDisplay = "N/A";
                string finalGradeDisplay = "N/A";

                string remarks = "Enrolled";
                decimal finalGradeValue = 0;
                bool hasGrade = false;

                if (enrollment.Grade != null)
                {
                    // 1. Midterm
                    if (enrollment.Grade.MidtermGrade.HasValue)
                    {
                        midtermDisplay = enrollment.Grade.MidtermGrade.Value.ToString("0.00");
                    }

                    // 2. Final Score (The grade for the final period)
                    if (enrollment.Grade.FinalScore.HasValue)
                    {
                        finalTermDisplay = enrollment.Grade.FinalScore.Value.ToString("0.00");
                    }

                    // 3. Final Grade (The computed grade)
                    if (enrollment.Grade.FinalGrade.HasValue)
                    {
                        finalGradeValue = enrollment.Grade.FinalGrade.Value;
                        finalGradeDisplay = finalGradeValue.ToString("0.00");
                        hasGrade = true;

                        // Pass/Fail Logic
                        if (finalGradeValue <= 3.0m) remarks = "Passed";
                        else remarks = "Failed";
                    }
                }

                GradeList.Add(new GradeViewModel
                {
                    SubjectCode = enrollment.Class.Subject.SubjectCode ?? "---",
                    Description = enrollment.Class.Subject.SubjectName ?? "Unknown",
                    Units = units,

                    Midterm = midtermDisplay,    // New
                    FinalTerm = finalTermDisplay,// New
                    FinalGrade = finalGradeDisplay, // Renamed from 'Grade'

                    Remarks = remarks,
                    Faculty = teacherName
                });

                // Update Stats
                TotalUnits += units;
                if (hasGrade)
                {
                    totalGradePoints += (finalGradeValue * units);
                    totalUnitsForGpa += units;

                    if (remarks == "Passed") SubjectsPassed++;
                    if (remarks == "Failed") SubjectsFailed++;
                }
            }

            if (totalUnitsForGpa > 0)
            {
                SemesterGPA = totalGradePoints / totalUnitsForGpa;
            }

            return Page();
        }
    }

    public class GradeViewModel
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Units { get; set; }

        public string Midterm { get; set; } = string.Empty; // New
        public string FinalTerm { get; set; } = string.Empty; // New
        public string FinalGrade { get; set; } = string.Empty; // Previously 'Grade'

        public string Remarks { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
    }
}