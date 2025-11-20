using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin.Curriculum
{
    // [Authorize(Roles = "Admin")]
    public class GradesModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public GradesModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)] public long SelectedYearId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedDepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedSubjectId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedClassId { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

        // --- Dropdown Lists ---
        public List<SelectListItem> SchoolYears { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Semesters { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableDepartments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableSubjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

        // --- Data & Display ---
        [BindProperty]
        public List<GradeEntry> Grades { get; set; } = new List<GradeEntry>();

        public string SubjectDescription { get; set; } = "Select a Subject";
        public string SelectedTermDisplay { get; set; } = "Select Term";

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();

            if (SelectedClassId > 0)
            {
                await LoadGradesAsync();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            SchoolYears = await _context.Schoolyears
                .OrderByDescending(y => y.Id)
                .Select(y => new SelectListItem { Value = y.Id.ToString(), Text = y.YearName, Selected = y.Id == SelectedYearId })
                .ToListAsync();

            if (SelectedYearId > 0)
            {
                Semesters = await _context.Semesters
                    .Where(s => s.SchoolYearId == SelectedYearId)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.SemesterName, Selected = s.Id == SelectedSemesterId })
                    .ToListAsync();
            }

            AvailableDepartments = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName, Selected = d.Id == SelectedDepartmentId })
                .ToListAsync();

            if (SelectedDepartmentId > 0)
            {
                AvailableSubjects = await _context.Subjects
                    .Where(s => s.DepartmentId == SelectedDepartmentId)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.SubjectCode} - {s.SubjectName}", Selected = s.Id == SelectedSubjectId })
                    .ToListAsync();
            }

            if (SelectedYearId > 0 && SelectedSemesterId > 0 && SelectedSubjectId > 0)
            {
                AvailableClasses = await _context.Classes
                    .Where(c => c.SchoolYearId == SelectedYearId && c.SemesterId == SelectedSemesterId && c.SubjectId == SelectedSubjectId)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ClassSection, Selected = c.Id == SelectedClassId })
                    .ToListAsync();

                var subject = await _context.Subjects.FindAsync(SelectedSubjectId);
                var sem = await _context.Semesters.FindAsync(SelectedSemesterId);
                var year = await _context.Schoolyears.FindAsync(SelectedYearId);

                if (subject != null) SubjectDescription = subject.SubjectName;
                if (sem != null && year != null) SelectedTermDisplay = $"{sem.SemesterName}, {year.YearName}";
            }
        }

        private async Task LoadGradesAsync()
        {
            var enrollments = await _context.Classenrollments
                .Include(e => e.Student)
                .Include(e => e.Grade)
                .Where(e => e.ClassId == SelectedClassId)
                .OrderBy(e => e.Student.LastName)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string lowerSearch = SearchTerm.ToLower();
                enrollments = enrollments
                    .Where(e => e.Student.LastName.ToLower().Contains(lowerSearch)
                             || e.Student.FirstName.ToLower().Contains(lowerSearch)
                             || e.Student.StudentNumber.ToLower().Contains(lowerSearch))
                    .ToList();
            }

            Grades = enrollments.Select(e => new GradeEntry
            {
                EnrollmentId = e.Id,
                GradeId = e.Grade?.Id,
                StudentIdNumber = e.Student.StudentNumber,
                StudentName = $"{e.Student.LastName}, {e.Student.FirstName}",
                Program = e.Student.CourseProgram ?? "N/A",
                Midterm = e.Grade?.MidtermGrade,
                Final = e.Grade?.FinalGrade,
                FinalScore = e.Grade?.FinalScore,
                Status = e.EnrollmentStatus
            }).ToList();
        }

        public async Task<IActionResult> OnPostUpdateAllAsync()
        {
            // --- VALIDATION LOGIC: Check Range 1.0 - 5.0 ---
            bool hasError = false;
            for (int i = 0; i < Grades.Count; i++)
            {
                var entry = Grades[i];
                if (IsGradeInvalid(entry.Midterm) || IsGradeInvalid(entry.Final) || IsGradeInvalid(entry.FinalScore))
                {
                    // Add error specifically for this row (if feasible) or general error
                    ModelState.AddModelError("", $"Error for {entry.StudentName}: Grades must be between 1.0 and 5.0.");
                    hasError = true;
                }
            }

            if (hasError || !ModelState.IsValid)
            {
                // Reload dropdowns and data so the page renders correctly with errors
                await OnGetAsync();
                return Page();
            }

            // If Validation Passes, Save Data
            foreach (var entry in Grades)
            {
                var enrollment = await _context.Classenrollments.FindAsync(entry.EnrollmentId);
                if (enrollment != null)
                {
                    enrollment.EnrollmentStatus = entry.Status;
                }

                var gradeRecord = await _context.Grades.FirstOrDefaultAsync(g => g.EnrollmentId == entry.EnrollmentId);

                if (gradeRecord == null)
                {
                    if (entry.Midterm.HasValue || entry.Final.HasValue || entry.FinalScore.HasValue)
                    {
                        var newGrade = new Grade
                        {
                            EnrollmentId = entry.EnrollmentId,
                            MidtermGrade = entry.Midterm,
                            FinalGrade = entry.Final,
                            FinalScore = entry.FinalScore
                        };
                        _context.Grades.Add(newGrade);
                    }
                }
                else
                {
                    gradeRecord.MidtermGrade = entry.Midterm;
                    gradeRecord.FinalGrade = entry.Final;
                    gradeRecord.FinalScore = entry.FinalScore;
                    _context.Grades.Update(gradeRecord);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Grades successfully saved.";
            return RedirectToPage(GetRouteData());
        }

        // Helper to validate range (Null is allowed, but numbers must be 1.0-5.0)
        private bool IsGradeInvalid(decimal? grade)
        {
            if (!grade.HasValue) return false; // Empty is OK
            return grade.Value < 1.0m || grade.Value > 5.0m;
        }

        private object GetRouteData() => new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId, SearchTerm };

        public class GradeEntry
        {
            public long EnrollmentId { get; set; }
            public long? GradeId { get; set; }
            public string StudentIdNumber { get; set; }
            public string StudentName { get; set; }
            public string Program { get; set; }

            // UPDATED: Added Range Attributes for Client-Side Validation (if using jquery-validation)
            [Range(1.0, 5.0, ErrorMessage = "1.0-5.0 only")]
            public decimal? Midterm { get; set; }

            [Range(1.0, 5.0, ErrorMessage = "1.0-5.0 only")]
            public decimal? Final { get; set; }

            [Range(1.0, 5.0, ErrorMessage = "1.0-5.0 only")]
            public decimal? FinalScore { get; set; }

            public string Status { get; set; }
        }
    }
}