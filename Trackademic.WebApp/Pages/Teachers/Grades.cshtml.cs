using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Teachers
{
    public class GradesModel : PageModel
    {
        // --- Bind properties ---
        [BindProperty(SupportsGet = true)] public string SchoolYear { get; set; }
        [BindProperty(SupportsGet = true)] public string Semester { get; set; }
        
        // Filter only by Class now
        [BindProperty(SupportsGet = true)] public string ClassId { get; set; } 
        
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }

        // --- Dropdowns ---
        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem> { new SelectListItem { Value = "2425", Text = "2425", Selected = true }, new SelectListItem { Value = "2324", Text = "2324" } };
        public List<SelectListItem> Semesters { get; } = new List<SelectListItem> { new SelectListItem { Value = "First", Text = "First", Selected = true }, new SelectListItem { Value = "Second", Text = "Second" } };
        
        // Available Classes for the specific teacher
        public List<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

        // --- Grade Data ---
        [BindProperty] public List<GradeEntry> Grades { get; set; } = new List<GradeEntry>();
        
        // Computed properties for Header display
        public string SubjectDescription { get; set; } 
        public string SubjectDisplayCode { get; set; }
        public string SelectedTermDisplay { get; set; }


        // --- MOCK DATA ---
        private static List<GradeEntry> _gradeData = new List<GradeEntry>
        {
            new GradeEntry { Id = 101, ClassId = "1", StudentIdNumber = "20210001", StudentName = "DELA CRUZ, JUAN P.", Program = "BSCPE", Midterm = "1.5", Final = "1.4", FinalGrade = 1.5, Status = "PASSED" },
            new GradeEntry { Id = 102, ClassId = "1", StudentIdNumber = "20210002", StudentName = "SANTOS, MARIA D.", Program = "BSCPE", Midterm = "2.0", Final = "2.1", FinalGrade = 2.1, Status = "PASSED" },
            new GradeEntry { Id = 103, ClassId = "1", StudentIdNumber = "20210008", StudentName = "REYES, MICHAEL A.", Program = "BSCPE", Midterm = "5.0", Final = "5.0", FinalGrade = 5.0, Status = "FAILED" },
            new GradeEntry { Id = 110, ClassId = "5", StudentIdNumber = "20210010", StudentName = "ESPINA, CARLO G.", Program = "BSCPE", Midterm = "INC", Final = "INC", FinalGrade = 0.0, Status = "INC" }
        };

        private static List<(string code, string dept, string title)> _subjectList = new List<(string, string, string)>
        {
            ("CPE335", "CPE", "Feedback & Control Systems"),
            ("CPE331", "CPE", "Data Communications"),
            ("ES038", "GE", "Technopreneurship"),
        };
        
        private static List<(string id, string subjectCode, string section)> _classList = new List<(string, string, string)>
        {
            ("1", "CPE335", "CPE335 - A-1 (M/W 8:00 AM)"),
            ("5", "ES038", "ES038 - Y-2 (T 1:00 PM)"),
        };

        public void OnGet()
        {
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            // 1. Load all classes available to this teacher (Mock: load all)
            AvailableClasses = _classList
                .Select(c => new SelectListItem { Value = c.id, Text = c.section })
                .ToList();

            // 2. Set default ClassId if none selected
            if (string.IsNullOrEmpty(ClassId) && AvailableClasses.Any()) 
                ClassId = AvailableClasses.First().Value;

            SelectedTermDisplay = $"{Semester} Semester, S.Y. {SchoolYear}";
            
            // 3. Derive Subject info from the Selected Class
            var selectedClass = _classList.FirstOrDefault(c => c.id == ClassId);
            if (selectedClass != default)
            {
                SubjectDisplayCode = selectedClass.subjectCode;
                var subj = _subjectList.FirstOrDefault(s => s.code == selectedClass.subjectCode);
                SubjectDescription = subj.title ?? "Unknown Subject";
            }
            else
            {
                SubjectDisplayCode = "";
                SubjectDescription = "Select a Class";
            }

            // 4. Filter Grades by ClassId
            var filteredGrades = _gradeData.Where(g => g.ClassId == ClassId);

            if (!string.IsNullOrEmpty(SearchTerm)) {
                string searchLower = SearchTerm.ToLower();
                filteredGrades = filteredGrades.Where(g => g.StudentName.ToLower().Contains(searchLower) || g.StudentIdNumber.ToLower().Contains(searchLower));
            }
            Grades = filteredGrades.OrderBy(g => g.StudentName).ToList();
        }

        public IActionResult OnPostUpdateAll()
        {
            if (!ModelState.IsValid) { OnGet(); return Page(); }
            foreach (var updatedGrade in Grades) {
                var existingGrade = _gradeData.FirstOrDefault(g => g.Id == updatedGrade.Id);
                if (existingGrade != null) {
                    existingGrade.Midterm = updatedGrade.Midterm; existingGrade.Final = updatedGrade.Final; existingGrade.FinalGrade = updatedGrade.FinalGrade; existingGrade.Status = updatedGrade.Status;
                }
            }
            return RedirectToPage("./Grade", new { SchoolYear, Semester, ClassId, SearchTerm });
        }

        public IActionResult OnPostDelete(int id)
        {
            var gradeToRemove = _gradeData.FirstOrDefault(g => g.Id == id);
            if (gradeToRemove != null) _gradeData.Remove(gradeToRemove);
            return RedirectToPage("./Grade", new { SchoolYear, Semester, ClassId, SearchTerm });
        }

        public class GradeEntry
        {
            public int Id { get; set; }
            // We don't need SubjectCode here anymore for filtering, just ClassId
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