using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Admin.Curriculum
{
    public class GradesModel : PageModel
    {
        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; }
        [BindProperty(SupportsGet = true)] public string Semester { get; set; }
        [BindProperty(SupportsGet = true)] public string DepartmentCode { get; set; } 
        [BindProperty(SupportsGet = true)] public string SubjectCode { get; set; } 
        [BindProperty(SupportsGet = true)] public string ClassId { get; set; } 
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }

        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem> { new SelectListItem { Value = "2425", Text = "2425", Selected = true }, new SelectListItem { Value = "2324", Text = "2324" } };
        public List<SelectListItem> Semesters { get; } = new List<SelectListItem> { new SelectListItem { Value = "First", Text = "First", Selected = true }, new SelectListItem { Value = "Second", Text = "Second" } };
        public List<SelectListItem> AvailableDepartments { get; } = new List<SelectListItem> { new SelectListItem { Value = "CPE", Text = "Computer Engineering" }, new SelectListItem { Value = "EE", Text = "Electrical Engineering" }, new SelectListItem { Value = "GE", Text = "General Education" } };
        public List<SelectListItem> AvailableSubjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

        [BindProperty] public List<GradeEntry> Grades { get; set; } = new List<GradeEntry>();
        public string SubjectDescription { get; set; } 
        public string SelectedTermDisplay { get; set; }

        private static List<GradeEntry> _gradeData = new List<GradeEntry>
        {
            new GradeEntry { Id = 101, SubjectCode = "CPE335", ClassId = "1", StudentIdNumber = "20210001", StudentName = "DELA CRUZ, JUAN P.", Program = "BSCPE", Midterm = "1.5", Final = "1.4", FinalGrade = 1.5, Status = "PASSED" },
            new GradeEntry { Id = 102, SubjectCode = "CPE335", ClassId = "1", StudentIdNumber = "20210002", StudentName = "SANTOS, MARIA D.", Program = "BSCPE", Midterm = "2.0", Final = "2.1", FinalGrade = 2.1, Status = "PASSED" },
            new GradeEntry { Id = 103, SubjectCode = "CPE335", ClassId = "1", StudentIdNumber = "20210008", StudentName = "REYES, MICHAEL A.", Program = "BSCPE", Midterm = "5.0", Final = "5.0", FinalGrade = 5.0, Status = "FAILED" },
            new GradeEntry { Id = 110, SubjectCode = "ES038", ClassId = "5", StudentIdNumber = "20210010", StudentName = "ESPINA, CARLO G.", Program = "BSCPE", Midterm = "INC", Final = "INC", FinalGrade = 0.0, Status = "INC" }
        };
        private static List<(string code, string dept, string title)> _subjectList = new List<(string, string, string)> { ("CPE335", "CPE", "Feedback & Control"), ("CPE331", "CPE", "Data Communications"), ("ES038", "GE", "Technopreneurship") };
        private static List<(string id, string subjectCode, string section)> _classList = new List<(string, string, string)> { ("1", "CPE335", "A-1 (M/W 8:00 AM)"), ("5", "ES038", "Y-2 (T 1:00 PM)") };

        public void OnGet()
        {
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";
            if (string.IsNullOrEmpty(DepartmentCode)) DepartmentCode = "CPE"; 

            LoadSubjectOptions();
            if (string.IsNullOrEmpty(SubjectCode) && AvailableSubjects.Any()) SubjectCode = AvailableSubjects.First().Value;
            
            LoadClassOptions();
            if (string.IsNullOrEmpty(ClassId) && AvailableClasses.Any()) ClassId = AvailableClasses.First().Value;

            SelectedTermDisplay = $"{Semester} Semester, S.Y. {SchoolYear}";
            var selectedSubject = _subjectList.FirstOrDefault(s => s.code == SubjectCode);
            SubjectDescription = selectedSubject.title ?? "Unknown Subject";

            var filteredGrades = _gradeData.Where(g => g.ClassId == ClassId);
            if (!string.IsNullOrEmpty(SearchTerm)) {
                string searchLower = SearchTerm.ToLower();
                filteredGrades = filteredGrades.Where(g => g.StudentName.ToLower().Contains(searchLower) || g.StudentIdNumber.ToLower().Contains(searchLower));
            }
            Grades = filteredGrades.OrderBy(g => g.StudentName).ToList();
        }
        
        private void LoadSubjectOptions() { AvailableSubjects = _subjectList.Where(s => s.dept == DepartmentCode).Select(s => new SelectListItem { Value = s.code, Text = $"{s.code} - {s.title}" }).ToList(); }
        private void LoadClassOptions() { if (string.IsNullOrEmpty(SubjectCode)) { AvailableClasses = new List<SelectListItem>(); return; } AvailableClasses = _classList.Where(c => c.subjectCode == SubjectCode).Select(c => new SelectListItem { Value = c.id, Text = c.section }).ToList(); }

        public IActionResult OnPostUpdateAll()
        {
            if (!ModelState.IsValid) { OnGet(); return Page(); }
            foreach (var updatedGrade in Grades) {
                var existingGrade = _gradeData.FirstOrDefault(g => g.Id == updatedGrade.Id);
                if (existingGrade != null) {
                    existingGrade.Midterm = updatedGrade.Midterm; existingGrade.Final = updatedGrade.Final; existingGrade.FinalGrade = updatedGrade.FinalGrade; existingGrade.Status = updatedGrade.Status;
                }
            }
            return RedirectToPage("./Grade", new { SchoolYear, Semester, DepartmentCode, SubjectCode, ClassId, SearchTerm });
        }

        public class GradeEntry
        {
            public int Id { get; set; }
            public string SubjectCode { get; set; } 
            public string DepartmentCode { get; set; }
            public string ClassId { get; set; }
            public string StudentIdNumber { get; set; }
            public string StudentName { get; set; }
            public string Program { get; set; }
            public string Midterm { get; set; }
            public string Final { get; set; }
            public double FinalGrade { get; set; }
            public string Status { get; set; }
        }
    }
}