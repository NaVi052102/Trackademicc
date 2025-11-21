using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trackademic.Data.Data;

namespace Trackademic.WebApp.Pages.Student
{
    // Display model for Enrolled Classes
    public class ClassEnrollmentDisplay
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int Units { get; set; } = 0;
        public string ImageUrl { get; set; } = "/images/class-default.png";

        // NEW: Helper to get Initials (e.g. "Data Structures" -> "DS")
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description)) return "??";
                var words = Description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                    return $"{words[0][0]}{words[1][0]}".ToUpper();
                else
                    return words[0].Length > 1 ? words[0].Substring(0, 2).ToUpper() : words[0].ToUpper();
            }
        }
    }

    // Display model for Performance Progress Bars
    public class PerformanceDisplay
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentGrade { get; set; }
        public int Percentage { get; set; } = 0;
    }

    public class StudentDashboardModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public StudentDashboardModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- VIEW PROPERTIES ---
        public Trackademic.Data.Models.Student? CurrentStudent { get; set; }

        public decimal CumulativeGPA { get; set; } = 0.0m;
        public int TotalUnitsEarned { get; set; } = 0;
        public int PassedSubjects { get; set; } = 0;
        public int TotalSubjects { get; set; } = 0;
        public int SubjectsFailed { get; set; } = 0; // Added missing property

        public Dictionary<string, decimal> GpaHistory { get; set; } = new();
        public List<ClassEnrollmentDisplay> EnrolledClasses { get; set; } = new();
        public List<PerformanceDisplay> PerformanceData { get; set; } = new();

        // --- MAIN HANDLER ---
        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out long studentId))
            {
                return RedirectToPage("/Account/Login");
            }

            // 1. Fetch Student Info
            CurrentStudent = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (CurrentStudent == null) return RedirectToPage("/Account/Login");

            // 2. Load Data
            await LoadDashboardData(studentId);

            return Page();
        }

        // --- HELPER METHODS ---

        private async Task LoadDashboardData(long studentId)
        {
            // 1. Fetch ALL Enrollments
            var allEnrollments = await _context.Classenrollments
                .Where(ce => ce.StudentId == studentId)
                .Include(ce => ce.Class).ThenInclude(c => c.Subject)
                .Include(ce => ce.Class).ThenInclude(c => c.SchoolYear)
                .Include(ce => ce.Class).ThenInclude(c => c.Semester)
                .Include(ce => ce.Class).ThenInclude(c => c.Classassignments).ThenInclude(ca => ca.Teacher)
                .Include(ce => ce.Grade)
                .ToListAsync();

            // A. CALCULATE CUMULATIVE STATS
            decimal totalGradePoints = 0;
            int totalUnitsForGpa = 0;
            PassedSubjects = 0;
            SubjectsFailed = 0;
            TotalUnitsEarned = 0;

            // Count Total Subjects (Exclude 'Enrolled')
            TotalSubjects = allEnrollments.Count(e => e.EnrollmentStatus != "Enrolled");

            foreach (var enrollment in allEnrollments)
            {
                if (enrollment.EnrollmentStatus == "Enrolled") continue;

                int units = enrollment.Class.Subject.CreditUnits ?? 3;

                if (enrollment.Grade != null && enrollment.Grade.FinalGrade.HasValue)
                {
                    decimal grade = enrollment.Grade.FinalGrade.Value;
                    totalGradePoints += (grade * units);
                    totalUnitsForGpa += units;

                    if (grade <= 3.0m)
                    {
                        PassedSubjects++;
                        TotalUnitsEarned += units;
                    }
                    else
                    {
                        SubjectsFailed++;
                    }
                }
            }

            if (totalUnitsForGpa > 0)
            {
                CumulativeGPA = totalGradePoints / totalUnitsForGpa;
            }

            // B. GPA HISTORY
            var groupedByTerm = allEnrollments
                .Where(e => e.EnrollmentStatus != "Enrolled")
                .GroupBy(e => new { Year = e.Class.SchoolYear.YearName, Sem = e.Class.Semester.SemesterName })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Sem);

            foreach (var term in groupedByTerm)
            {
                decimal termPoints = 0;
                int termUnits = 0;

                foreach (var item in term)
                {
                    if (item.Grade != null && item.Grade.FinalGrade.HasValue)
                    {
                        int u = item.Class.Subject.CreditUnits ?? 3;
                        termPoints += (item.Grade.FinalGrade.Value * u);
                        termUnits += u;
                    }
                }

                if (termUnits > 0)
                {
                    string key = $"{term.Key.Year} - {term.Key.Sem}";
                    if (!GpaHistory.ContainsKey(key))
                    {
                        GpaHistory.Add(key, termPoints / termUnits);
                    }
                }
            }

            // C. CURRENT/LATEST ENROLLMENTS LIST
            var latestEnrollments = allEnrollments
                .OrderByDescending(e => e.Class.SchoolYear.YearName)
                .ThenByDescending(e => e.Class.Semester.Id)
                .Take(5)
                .ToList();

            EnrolledClasses = latestEnrollments.Select(e => new ClassEnrollmentDisplay
            {
                SubjectCode = e.Class.Subject.SubjectCode ?? "N/A",
                Description = e.Class.Subject.SubjectName ?? "Unknown",
                Units = e.Class.Subject.CreditUnits ?? 0,
                FacultyName = e.Class.Classassignments.FirstOrDefault()?.Teacher?.LastName ?? "TBA",
                ImageUrl = "/images/class-default.png"
            }).ToList();

            // D. PERFORMANCE DATA
            foreach (var item in latestEnrollments)
            {
                if (item.Grade != null && item.Grade.FinalGrade.HasValue)
                {
                    decimal g = item.Grade.FinalGrade.Value;
                    int percent = (int)((5.0m - g) / 4.0m * 100m);
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;

                    PerformanceData.Add(new PerformanceDisplay
                    {
                        SubjectCode = item.Class.Subject.SubjectCode ?? "N/A",
                        Description = item.Class.Subject.SubjectName ?? "Unknown",
                        CurrentGrade = g,
                        Percentage = percent
                    });
                }
            }
        }
    }
}