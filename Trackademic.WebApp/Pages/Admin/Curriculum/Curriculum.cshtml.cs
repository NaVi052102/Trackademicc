using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin.Curriculum
{
    // [Authorize(Roles = "Admin")] // Uncomment to secure
    public class CurriculumModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public CurriculumModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Selection State (Maintains filter state across reloads) ---
        [BindProperty(SupportsGet = true)] public int SelectedYearId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedDepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSubjectId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedClassId { get; set; }

        // --- Data Lists ---
        public IList<SchoolYearViewModel> YearList { get; set; } = new List<SchoolYearViewModel>();
        public IList<SemesterViewModel> SemesterList { get; set; } = new List<SemesterViewModel>();
        public IList<DepartmentViewModel> DepartmentList { get; set; } = new List<DepartmentViewModel>();
        public IList<SubjectViewModel> SubjectList { get; set; } = new List<SubjectViewModel>();
        public IList<ClassViewModel> ClassList { get; set; } = new List<ClassViewModel>();
        public SelectList TeacherOptions { get; set; }

        [BindProperty]
        public AssignmentViewModel Assignment { get; set; } = new AssignmentViewModel();

        // --- View Models ---
        public class SchoolYearViewModel { public long Id { get; set; } public string YearName { get; set; } public DateTime? DateStarted { get; set; } public DateTime? DateEnded { get; set; } }
        public class SemesterViewModel { public long Id { get; set; } public long SchoolYearId { get; set; } public string SemesterName { get; set; } public DateTime? DateStarted { get; set; } public DateTime? DateEnded { get; set; } }
        public class DepartmentViewModel { public long Id { get; set; } public string DeptName { get; set; } }
        public class SubjectViewModel { public long Id { get; set; } public long DepartmentId { get; set; } public string SubjectCode { get; set; } public string SubjectName { get; set; } }
        public class ClassViewModel { public long Id { get; set; } public long SubjectId { get; set; } public long SchoolYearId { get; set; } public long SemesterId { get; set; } public string ClassSection { get; set; } public string AssignedTeacherName { get; set; } = "Unassigned"; public long? AssignedTeacherId { get; set; } }
        public class AssignmentViewModel { public long ClassId { get; set; } [Required] public long TeacherId { get; set; } }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // 1. Years
            YearList = await _context.Schoolyears
                .OrderByDescending(y => y.Id)
                .Select(y => new SchoolYearViewModel
                {
                    Id = y.Id,
                    YearName = y.YearName,
                    DateStarted = y.DateStarted.HasValue ? y.DateStarted.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    DateEnded = y.DateEnded.HasValue ? y.DateEnded.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
                })
                .ToListAsync();

            // 2. Semesters (Dependent on SelectedYearId)
            if (SelectedYearId > 0)
            {
                SemesterList = await _context.Semesters
                    .Where(s => s.SchoolYearId == SelectedYearId)
                    .Select(s => new SemesterViewModel
                    {
                        Id = s.Id,
                        SchoolYearId = s.SchoolYearId,
                        SemesterName = s.SemesterName,
                        DateStarted = s.DateStarted.HasValue ? s.DateStarted.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        DateEnded = s.DateEnded.HasValue ? s.DateEnded.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
                    })
                    .ToListAsync();
            }

            // 3. Departments
            DepartmentList = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new DepartmentViewModel { Id = d.Id, DeptName = d.DeptName })
                .ToListAsync();

            // 4. Subjects (Dependent on SelectedDepartmentId)
            if (SelectedDepartmentId > 0)
            {
                SubjectList = await _context.Subjects
                    .Where(s => s.DepartmentId == SelectedDepartmentId)
                    .Select(s => new SubjectViewModel { Id = s.Id, DepartmentId = s.DepartmentId, SubjectCode = s.SubjectCode, SubjectName = s.SubjectName })
                    .ToListAsync();
            }

            // 5. Classes (Dependent on Year, Semester, and Subject)
            if (SelectedYearId > 0 && SelectedSemesterId > 0 && SelectedSubjectId > 0)
            {
                // We fetch classes and manually join with ClassAssignments to get the teacher name
                // Note: Assuming 1 teacher per class for this view
                var classesData = await _context.Classes
                    .Where(c => c.SchoolYearId == SelectedYearId && c.SemesterId == SelectedSemesterId && c.SubjectId == SelectedSubjectId)
                    .Include(c => c.Classassignments)
                        .ThenInclude(ca => ca.Teacher)
                    .ToListAsync();

                ClassList = classesData.Select(c =>
                {
                    var assignment = c.Classassignments.FirstOrDefault();
                    return new ClassViewModel
                    {
                        Id = c.Id,
                        SubjectId = c.SubjectId,
                        SchoolYearId = c.SchoolYearId,
                        SemesterId = c.SemesterId,
                        ClassSection = c.ClassSection,
                        AssignedTeacherId = assignment?.TeacherId,
                        AssignedTeacherName = assignment != null ? $"{assignment.Teacher.FirstName} {assignment.Teacher.LastName}" : "Unassigned"
                    };
                }).ToList();
            }

            // 6. Teachers for Dropdown
            var teachers = await _context.Teachers.Select(t => new { t.Id, Name = $"{t.FirstName} {t.LastName}" }).ToListAsync();
            TeacherOptions = new SelectList(teachers, "Id", "Name");
        }

        // ====================================================================
        // VALIDATION LOGIC
        // ====================================================================
        private bool IsYearFormatValid(string yearName, DateTime? start, DateTime? end, out string error)
        {
            error = string.Empty;
            if (!Regex.IsMatch(yearName, @"^\d{4}-\d{4}$")) { error = "Format must be YYYY-YYYY."; return false; }
            var parts = yearName.Split('-');
            if (int.Parse(parts[1]) != int.Parse(parts[0]) + 1) { error = "Year gap must be exactly 1 year."; return false; }
            if (start >= end) { error = "End Date must be after Start Date."; return false; }
            return true;
        }

        private bool IsTextReasonable(string text, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2) { error = "Name/Title must be at least 2 characters."; return false; }
            return true;
        }

        private object GetRouteData() => new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId };

        // ====================================================================
        // HANDLERS
        // ====================================================================

        // 1. SCHOOL YEARS
        public async Task<IActionResult> OnPostCreateYearAsync(string yearName, DateTime dateStarted, DateTime dateEnded)
        {
            if (!IsYearFormatValid(yearName, dateStarted, dateEnded, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            _context.Schoolyears.Add(new Schoolyear
            {
                YearName = yearName,
                DateStarted = DateOnly.FromDateTime(dateStarted),
                DateEnded = DateOnly.FromDateTime(dateEnded)
            });
            await _context.SaveChangesAsync();
            TempData["Message"] = "School Year added.";
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostEditYearAsync(long id, string yearName, DateTime dateStarted, DateTime dateEnded)
        {
            if (!IsYearFormatValid(yearName, dateStarted, dateEnded, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            var item = await _context.Schoolyears.FindAsync(id);
            if (item != null)
            {
                item.YearName = yearName;
                item.DateStarted = DateOnly.FromDateTime(dateStarted);
                item.DateEnded = DateOnly.FromDateTime(dateEnded);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostDeleteYearAsync(long id)
        {
            var item = await _context.Schoolyears.FindAsync(id);
            if (item != null) { _context.Schoolyears.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToPage(new { SelectedYearId = 0 });
        }

        // 2. SEMESTERS
        public async Task<IActionResult> OnPostCreateSemesterAsync(long schoolYearId, string semesterName, DateTime dateStarted, DateTime dateEnded)
        {
            if (!IsTextReasonable(semesterName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            if (dateStarted >= dateEnded) { TempData["Error"] = "End Date must be after Start Date."; return RedirectToPage(GetRouteData()); }

            _context.Semesters.Add(new Semester
            {
                SchoolYearId = schoolYearId,
                SemesterName = semesterName,
                DateStarted = DateOnly.FromDateTime(dateStarted),
                DateEnded = DateOnly.FromDateTime(dateEnded)
            });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Semester added.";
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostEditSemesterAsync(long id, string semesterName, DateTime dateStarted, DateTime dateEnded)
        {
            if (!IsTextReasonable(semesterName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            var item = await _context.Semesters.FindAsync(id);
            if (item != null)
            {
                item.SemesterName = semesterName;
                item.DateStarted = DateOnly.FromDateTime(dateStarted);
                item.DateEnded = DateOnly.FromDateTime(dateEnded);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostDeleteSemesterAsync(long id)
        {
            var item = await _context.Semesters.FindAsync(id);
            if (item != null) { _context.Semesters.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId = 0 });
        }

        // 3. DEPARTMENTS
        public async Task<IActionResult> OnPostCreateDepartmentAsync(string deptName)
        {
            if (!IsTextReasonable(deptName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            _context.Departments.Add(new Department { DeptName = deptName });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Department added.";
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(long id, string deptName)
        {
            if (!IsTextReasonable(deptName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            var item = await _context.Departments.FindAsync(id);
            if (item != null) { item.DeptName = deptName; await _context.SaveChangesAsync(); }
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostDeleteDepartmentAsync(long id)
        {
            var item = await _context.Departments.FindAsync(id);
            if (item != null) { _context.Departments.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId = 0 });
        }

        // 4. SUBJECTS
        public async Task<IActionResult> OnPostCreateSubjectAsync(long departmentId, string subjectCode, string subjectName)
        {
            if (!IsTextReasonable(subjectCode, out string err) || !IsTextReasonable(subjectName, out string err2))
            { TempData["Error"] = "Code and Name must be valid."; return RedirectToPage(GetRouteData()); }

            _context.Subjects.Add(new Subject { DepartmentId = departmentId, SubjectCode = subjectCode, SubjectName = subjectName });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Subject added.";
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostEditSubjectAsync(long id, string subjectCode, string subjectName)
        {
            var item = await _context.Subjects.FindAsync(id);
            if (item != null) { item.SubjectCode = subjectCode; item.SubjectName = subjectName; await _context.SaveChangesAsync(); }
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(long id)
        {
            var item = await _context.Subjects.FindAsync(id);
            if (item != null) { _context.Subjects.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId = 0 });
        }

        // 5. CLASSES
        public async Task<IActionResult> OnPostCreateClassAsync(long subjectId, long yearId, long semesterId, string classSection)
        {
            if (!IsTextReasonable(classSection, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            // Ensure uniqueness (Subject + Year + Sem + Section)
            bool exists = await _context.Classes.AnyAsync(c => c.SubjectId == subjectId && c.SchoolYearId == yearId && c.SemesterId == semesterId && c.ClassSection == classSection);
            if (exists) { TempData["Error"] = "This class section already exists for this subject/term."; return RedirectToPage(GetRouteData()); }

            _context.Classes.Add(new Class { SubjectId = subjectId, SchoolYearId = yearId, SemesterId = semesterId, ClassSection = classSection });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Class added.";
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostEditClassAsync(long id, string classSection)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item != null) { item.ClassSection = classSection; await _context.SaveChangesAsync(); }
            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostDeleteClassAsync(long id)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item != null) { _context.Classes.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId = 0 });
        }

        // 6. ASSIGN TEACHER
        public async Task<IActionResult> OnPostAssignTeacherAsync()
        {
            if (Assignment.ClassId == 0 || Assignment.TeacherId == 0) { TempData["Error"] = "Please select a class and a teacher."; return RedirectToPage(GetRouteData()); }

            // Logic: Check if assignment exists. If yes, update. If no, create.
            var existingAssignment = await _context.Classassignments.FirstOrDefaultAsync(ca => ca.ClassId == Assignment.ClassId);

            if (existingAssignment != null)
            {
                // Update existing
                existingAssignment.TeacherId = Assignment.TeacherId;
                TempData["Message"] = "Teacher assignment updated.";
            }
            else
            {
                // Create new
                _context.Classassignments.Add(new Classassignment { ClassId = Assignment.ClassId, TeacherId = Assignment.TeacherId });
                TempData["Message"] = "Teacher assigned successfully.";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(GetRouteData());
        }
    }
}