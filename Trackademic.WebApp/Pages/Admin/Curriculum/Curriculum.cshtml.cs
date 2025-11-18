using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Trackademic.WebApp.Pages.Admin.Curriculum
{
    public class CurriculumModel : PageModel
    {
        // --- Selection State ---
        [BindProperty(SupportsGet = true)] public int SelectedYearId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedDepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSubjectId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedClassId { get; set; }

        // --- Data Lists ---
        public IList<SchoolYearViewModel> YearList { get; set; }
        public IList<SemesterViewModel> SemesterList { get; set; }
        public IList<DepartmentViewModel> DepartmentList { get; set; }
        public IList<SubjectViewModel> SubjectList { get; set; } = new List<SubjectViewModel>();
        public IList<ClassViewModel> ClassList { get; set; } = new List<ClassViewModel>();
        public SelectList TeacherOptions { get; set; }

        [BindProperty]
        public AssignmentViewModel Assignment { get; set; } = new AssignmentViewModel();

        // --- View Models ---
        public class SchoolYearViewModel { public int Id { get; set; } public string YearName { get; set; } = string.Empty; public DateTime? DateStarted { get; set; } public DateTime? DateEnded { get; set; } }
        public class SemesterViewModel { public int Id { get; set; } public int SchoolYearId { get; set; } public string SemesterName { get; set; } = string.Empty; public DateTime? DateStarted { get; set; } public DateTime? DateEnded { get; set; } }
        public class DepartmentViewModel { public int Id { get; set; } public string DeptName { get; set; } = string.Empty; }
        public class SubjectViewModel { public int Id { get; set; } public int DepartmentId { get; set; } public string SubjectCode { get; set; } = string.Empty; public string SubjectName { get; set; } = string.Empty; }
        public class ClassViewModel { public int Id { get; set; } public int SubjectId { get; set; } public int SchoolYearId { get; set; } public int SemesterId { get; set; } public string ClassSection { get; set; } = string.Empty; public string AssignedTeacherName { get; set; } = "Unassigned"; public int? AssignedTeacherId { get; set; } }
        public class AssignmentViewModel { public int ClassId { get; set; } [Required] public int TeacherId { get; set; } }
        
        // --- Mock Data ---
        private static List<SchoolYearViewModel> StaticYears = new List<SchoolYearViewModel> { new SchoolYearViewModel { Id = 1, YearName = "2024-2025", DateStarted = new DateTime(2024, 8, 1), DateEnded = new DateTime(2025, 5, 30) } };
        private static List<SemesterViewModel> StaticSemesters = new List<SemesterViewModel> { new SemesterViewModel { Id = 10, SchoolYearId = 1, SemesterName = "1st Semester", DateStarted = new DateTime(2024, 8, 1), DateEnded = new DateTime(2024, 12, 20) } };
        private static List<DepartmentViewModel> StaticDepartments = new List<DepartmentViewModel> { new DepartmentViewModel { Id = 50, DeptName = "Computer Engineering" } };
        private static List<SubjectViewModel> StaticSubjects = new List<SubjectViewModel> { new SubjectViewModel { Id = 101, DepartmentId = 50, SubjectCode = "CPE 101", SubjectName = "Intro to Programming" } };
        private static List<ClassViewModel> StaticClasses = new List<ClassViewModel> { new ClassViewModel { Id = 5001, SubjectId = 101, SchoolYearId = 1, SemesterId = 10, ClassSection = "CPE-1A", AssignedTeacherName = "Dr. Smith", AssignedTeacherId = 1 } };
        
        public async Task OnGetAsync() { LoadData(); await Task.CompletedTask; }

        private void LoadData()
        {
            YearList = StaticYears; 
            SemesterList = SelectedYearId > 0 ? StaticSemesters.Where(s => s.SchoolYearId == SelectedYearId).ToList() : new List<SemesterViewModel>();
            DepartmentList = StaticDepartments;
            SubjectList = SelectedDepartmentId > 0 ? StaticSubjects.Where(s => s.DepartmentId == SelectedDepartmentId).ToList() : new List<SubjectViewModel>();
            ClassList = (SelectedSubjectId > 0 && SelectedYearId > 0 && SelectedSemesterId > 0) ? StaticClasses.Where(c => c.SubjectId == SelectedSubjectId && c.SchoolYearId == SelectedYearId && c.SemesterId == SelectedSemesterId).ToList() : new List<ClassViewModel>();
            TeacherOptions = GetMockTeacherOptions();
        }

        // ====================================================================
        // VALIDATION LOGIC (Server-Side)
        // ====================================================================
        private bool IsYearFormatValid(string yearName, DateTime? start, DateTime? end, out string error)
        {
            error = string.Empty;
            // 1. Regex Format
            if (!Regex.IsMatch(yearName, @"^\d{4}-\d{4}$")) { error = "Format must be YYYY-YYYY."; return false; }
            
            // 2. Year Gap
            var parts = yearName.Split('-');
            int startY = int.Parse(parts[0]);
            int endY = int.Parse(parts[1]);
            if (endY != startY + 1) { error = "School Year gap must be exactly 1 year."; return false; }

            // 3. Date Logic
            if (start >= end) { error = "End Date must be after Start Date."; return false; }
            
            // 4. Reasonable Date Check (Start date should match the Year Name start)
            if (start.Value.Year != startY) { error = $"Start Date year ({start.Value.Year}) does not match the Academic Year start ({startY})."; return false; }

            return true;
        }

        private bool IsTextReasonable(string text, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2)
            {
                error = "Name/Title must be at least 2 characters long.";
                return false;
            }
            return true;
        }

        // ====================================================================
        // HANDLERS
        // ====================================================================

        // 1. SCHOOL YEARS
        public IActionResult OnPostCreateYear(string yearName, DateTime? dateStarted, DateTime? dateEnded)
        {
            if (!IsYearFormatValid(yearName, dateStarted, dateEnded, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            
            int newId = StaticYears.Any() ? StaticYears.Max(y => y.Id) + 1 : 1;
            StaticYears.Add(new SchoolYearViewModel { Id = newId, YearName = yearName, DateStarted = dateStarted, DateEnded = dateEnded });
            return RedirectToPage(new { SelectedYearId = newId });
        }
        public IActionResult OnPostEditYear(int id, string yearName, DateTime? dateStarted, DateTime? dateEnded)
        {
            if (!IsYearFormatValid(yearName, dateStarted, dateEnded, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }

            var item = StaticYears.FirstOrDefault(x => x.Id == id);
            if(item != null) { item.YearName = yearName; item.DateStarted = dateStarted; item.DateEnded = dateEnded; }
            return RedirectToPage(GetRouteData());
        }
        public IActionResult OnPostDeleteYear(int id) { StaticYears.RemoveAll(y => y.Id == id); return RedirectToPage(new { SelectedYearId = 0 }); }

        // 2. SEMESTERS
        public IActionResult OnPostCreateSemester(int schoolYearId, string semesterName, DateTime? dateStarted, DateTime? dateEnded)
        {
            if (!IsTextReasonable(semesterName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            if (dateStarted >= dateEnded) { TempData["Error"] = "Semester End Date must be after Start Date."; return RedirectToPage(GetRouteData()); }

            int newId = StaticSemesters.Any() ? StaticSemesters.Max(s => s.Id) + 1 : 1;
            StaticSemesters.Add(new SemesterViewModel { Id = newId, SchoolYearId = schoolYearId, SemesterName = semesterName, DateStarted = dateStarted, DateEnded = dateEnded });
            return RedirectToPage(new { SelectedYearId = schoolYearId, SelectedSemesterId = newId });
        }
        public IActionResult OnPostEditSemester(int id, string semesterName, DateTime? dateStarted, DateTime? dateEnded)
        {
            if (!IsTextReasonable(semesterName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            if (dateStarted >= dateEnded) { TempData["Error"] = "Semester End Date must be after Start Date."; return RedirectToPage(GetRouteData()); }

            var item = StaticSemesters.FirstOrDefault(x => x.Id == id);
            if(item != null) { item.SemesterName = semesterName; item.DateStarted = dateStarted; item.DateEnded = dateEnded; }
            return RedirectToPage(GetRouteData());
        }
        public IActionResult OnPostDeleteSemester(int id) { StaticSemesters.RemoveAll(s => s.Id == id); return RedirectToPage(new { SelectedYearId, SelectedSemesterId = 0 }); }

        // 3. DEPARTMENTS
        public IActionResult OnPostCreateDepartment(string deptName)
        {
            if (!IsTextReasonable(deptName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            int newId = StaticDepartments.Any() ? StaticDepartments.Max(d => d.Id) + 1 : 1;
            StaticDepartments.Add(new DepartmentViewModel { Id = newId, DeptName = deptName });
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId = newId });
        }
        public IActionResult OnPostEditDepartment(int id, string deptName)
        {
            if (!IsTextReasonable(deptName, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            var item = StaticDepartments.FirstOrDefault(x => x.Id == id); if(item != null) { item.DeptName = deptName; }
            return RedirectToPage(GetRouteData());
        }
        public IActionResult OnPostDeleteDepartment(int id) { StaticDepartments.RemoveAll(d => d.Id == id); return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId = 0 }); }

        // 4. SUBJECTS
        public IActionResult OnPostCreateSubject(int departmentId, string subjectCode, string subjectName)
        {
            if (!IsTextReasonable(subjectCode, out string err) || !IsTextReasonable(subjectName, out string err2)) { TempData["Error"] = "Code and Name must be at least 2 characters."; return RedirectToPage(GetRouteData()); }
            int newId = StaticSubjects.Any() ? StaticSubjects.Max(s => s.Id) + 1 : 1;
            StaticSubjects.Add(new SubjectViewModel { Id = newId, DepartmentId = departmentId, SubjectCode = subjectCode, SubjectName = subjectName });
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId = departmentId, SelectedSubjectId = newId });
        }
        public IActionResult OnPostEditSubject(int id, string subjectCode, string subjectName)
        {
            if (!IsTextReasonable(subjectCode, out string err) || !IsTextReasonable(subjectName, out string err2)) { TempData["Error"] = "Code and Name must be at least 2 characters."; return RedirectToPage(GetRouteData()); }
            var item = StaticSubjects.FirstOrDefault(x => x.Id == id); if(item != null) { item.SubjectCode = subjectCode; item.SubjectName = subjectName; }
            return RedirectToPage(GetRouteData());
        }
        public IActionResult OnPostDeleteSubject(int id) { StaticSubjects.RemoveAll(s => s.Id == id); return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId = 0 }); }

        // 5. CLASSES
        public IActionResult OnPostCreateClass(int subjectId, int yearId, int semesterId, string classSection)
        {
            if (!IsTextReasonable(classSection, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            int newId = StaticClasses.Any() ? StaticClasses.Max(c => c.Id) + 1 : 1;
            StaticClasses.Add(new ClassViewModel { Id = newId, SubjectId = subjectId, SchoolYearId = yearId, SemesterId = semesterId, ClassSection = classSection, AssignedTeacherName = "Unassigned" });
            return RedirectToPage(new { SelectedYearId = yearId, SelectedSemesterId = semesterId, SelectedDepartmentId, SelectedSubjectId = subjectId, SelectedClassId = newId });
        }
        public IActionResult OnPostEditClass(int id, string classSection)
        {
            if (!IsTextReasonable(classSection, out string err)) { TempData["Error"] = err; return RedirectToPage(GetRouteData()); }
            var item = StaticClasses.FirstOrDefault(x => x.Id == id); if(item != null) { item.ClassSection = classSection; }
            return RedirectToPage(GetRouteData());
        }
        public IActionResult OnPostDeleteClass(int id) { StaticClasses.RemoveAll(c => c.Id == id); return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId = 0 }); }

        // 6. ASSIGN TEACHER
        public IActionResult OnPostAssignTeacher()
        {
            if (Assignment.ClassId == 0 || Assignment.TeacherId == 0) { TempData["Error"] = "Please select a class and a teacher."; return RedirectToPage(GetRouteData()); }
            var targetClass = StaticClasses.FirstOrDefault(c => c.Id == Assignment.ClassId);
            var teacherName = GetMockTeacherOptions().FirstOrDefault(t => t.Value == Assignment.TeacherId.ToString())?.Text;
            if (targetClass != null) { targetClass.AssignedTeacherId = Assignment.TeacherId; targetClass.AssignedTeacherName = teacherName; TempData["Message"] = $"Assigned {teacherName} to {targetClass.ClassSection}"; }
            return RedirectToPage(GetRouteData());
        }
        
        private object GetRouteData() { return new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId }; }
        private SelectList GetMockTeacherOptions() { return new SelectList(new List<(int Id, string Name)> { (1, "Dr. Smith"), (2, "Prof. Johnson"), (3, "Ms. Evans") }, "Id", "Name"); }
    }
}