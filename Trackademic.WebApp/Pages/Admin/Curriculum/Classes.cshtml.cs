using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Admin.Curriculum    
{
    public class ClassModel : PageModel
    {
        // --- Filter/Binding Properties ---
        [BindProperty(SupportsGet = true)] public int SelectedYearId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedDepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedSubjectId { get; set; }
        [BindProperty(SupportsGet = true)] public int SelectedClassId { get; set; } 

        // --- Enrollment State ---
        [BindProperty(SupportsGet = true)]
        public bool IsEnrollmentMode { get; set; } = false; 

        [BindProperty]
        public string StudentSearchTerm { get; set; }

        // --- Dropdowns ---
        public List<SelectListItem> YearOptions { get; set; }
        public List<SelectListItem> SemesterOptions { get; set; }
        public List<SelectListItem> DepartmentOptions { get; set; }
        public List<SelectListItem> SubjectOptions { get; set; }
        public List<SelectListItem> ClassOptions { get; set; }

        // --- View Data ---
        public ClassDetailsViewModel SelectedClassDetails { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
        public List<StudentRosterViewModel> EnrollmentSearchResults { get; set; } = new List<StudentRosterViewModel>();

        public void OnGet()
        {
            LoadAllData();
        }

        // --- HANDLERS ---
        public IActionResult OnPostEnrollMode() { LoadAllData(); IsEnrollmentMode = true; return Page(); }

        public IActionResult OnPostSearchStudent()
        {
            LoadAllData();
            IsEnrollmentMode = true;
            if (!string.IsNullOrEmpty(StudentSearchTerm))
            {
                // Mock Search
                if (StudentSearchTerm.ToLower().Contains("gomez")) EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "S006", FullName = "Gomez, Mark D." });
                else if (StudentSearchTerm.ToLower().Contains("tan")) EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "T001", FullName = "Tan, Lily A." });
            }
            return Page();
        }

        public IActionResult OnPostEnrollStudent(string studentIdToEnroll)
        {
            TempData["Message"] = $"Student {studentIdToEnroll} enrolled successfully.";
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId });
        }
        
        public IActionResult OnPostRemoveStudent(string studentId)
        {
            TempData["Message"] = $"Student {studentId} removed from class.";
            return RedirectToPage(new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId });
        }

        // --- LOADING HELPERS ---
        private void LoadAllData()
        {
            // 1. Load Years (No default selection)
            YearOptions = GetMockYears().Select(y => new SelectListItem { Value = y.YearID.ToString(), Text = y.YearTitle }).ToList();

            // 2. Load Semesters (No default selection)
            SemesterOptions = GetMockSemesters().Select(s => new SelectListItem { Value = s.SemesterID.ToString(), Text = s.Title }).ToList();

            // 3. Load Departments
            LoadDepartmentOptions();

            // 4. Load Subjects (Dependent on filters)
            LoadSubjectOptions();

            // 5. Load Classes (Dependent on filters)
            LoadClassOptions();

            // 6. Load Details (Only if a class is finally selected)
            if (SelectedClassId > 0)
            {
                LoadClassDetails(SelectedClassId);
                LoadStudentRoster(SelectedClassId);
            }
        }

        private void LoadDepartmentOptions()
        {
            // Only load departments if year/sem are selected (optional logic, but keeps it clean)
            // For now, we load them always to allow selection
            var departments = GetMockDepartments();
            DepartmentOptions = departments.Select(d => new SelectListItem { Value = d.DepartmentID.ToString(), Text = d.Title }).ToList();
        }

        private void LoadSubjectOptions()
        {
            // Requires Year, Sem, and Dept to be selected first
            if (SelectedYearId == 0 || SelectedSemesterId == 0 || SelectedDepartmentId == 0)
            {
                SubjectOptions = new List<SelectListItem>();
                return;
            }

            var subjects = GetMockSubjects().Where(s => s.YearID == SelectedYearId && s.SemesterID == SelectedSemesterId && s.DepartmentID == SelectedDepartmentId).ToList();
            SubjectOptions = subjects.Select(s => new SelectListItem { Value = s.SubjectID.ToString(), Text = $"{s.Code} - {s.Title}" }).ToList();
        }

        private void LoadClassOptions()
        {
            // Requires Subject to be selected
            if (SelectedSubjectId == 0) 
            { 
                ClassOptions = new List<SelectListItem>(); 
                return; 
            }

            var classes = GetMockClasses().Where(c => c.SubjectID == SelectedSubjectId).ToList();
            ClassOptions = classes.Select(c => new SelectListItem { Value = c.ClassID.ToString(), Text = $"{c.SectionName} ({c.Schedule})" }).ToList();
        }
        
        private void LoadClassDetails(int classId)
        {
            var classData = GetMockClasses().FirstOrDefault(c => c.ClassID == classId);
            var subjectData = GetMockSubjects().FirstOrDefault(s => s.SubjectID == classData?.SubjectID);

            if (classData != null && subjectData != null)
            {
                SelectedClassDetails = new ClassDetailsViewModel
                {
                    SubjectCode = subjectData.Code,
                    SectionName = classData.SectionName,
                    CourseTitle = subjectData.Title,
                    TotalStudents = GetMockStudents().Count(s => s.ClassID == classId),
                    AssignedTeacher = classData.AssignedTeacher,
                    // Schedule removed
                };
            }
        }

        private void LoadStudentRoster(int classId)
        {
            Students = GetMockStudents().Where(s => s.ClassID == classId).ToList();
        }
        
        // --- MODELS & MOCKS ---
        public class SchoolYearViewModel { public int YearID { get; set; } public string YearTitle { get; set; } }
        public class SemesterViewModel { public int SemesterID { get; set; } public string Title { get; set; } }
        public class DepartmentViewModel { public int DepartmentID { get; set; } public string Title { get; set; } }
        public class SubjectViewModel { public int SubjectID { get; set; } public int YearID { get; set; } public int SemesterID { get; set; } public int DepartmentID { get; set; } public string Code { get; set; } public string Title { get; set; } }
        public class ClassCardViewModel { public int ClassID { get; set; } public int SubjectID { get; set; } public string SectionName { get; set; } public string Schedule { get; set; } public string AssignedTeacher { get; set; } }
        
        public class ClassDetailsViewModel
        {
            public string SubjectCode { get; set; }
            public string SectionName { get; set; }
            public string CourseTitle { get; set; }
            public int TotalStudents { get; set; }
            public string AssignedTeacher { get; set; }
        }
        
        public class StudentRosterViewModel { public int ClassID { get; set; } public string StudentId { get; set; } public string FullName { get; set; } } 

        private IList<SchoolYearViewModel> GetMockYears() => new List<SchoolYearViewModel> { new SchoolYearViewModel { YearID = 1, YearTitle = "2024-2025" }, new SchoolYearViewModel { YearID = 2, YearTitle = "2023-2024" } };
        private IList<SemesterViewModel> GetMockSemesters() => new List<SemesterViewModel> { new SemesterViewModel { SemesterID = 10, Title = "1st Semester" }, new SemesterViewModel { SemesterID = 20, Title = "2nd Semester" } };
        private IList<DepartmentViewModel> GetMockDepartments() => new List<DepartmentViewModel> { new DepartmentViewModel { DepartmentID = 50, Title = "Computer Science" }, new DepartmentViewModel { DepartmentID = 70, Title = "General Education" } };
        private IList<SubjectViewModel> GetMockSubjects() => new List<SubjectViewModel> { 
            new SubjectViewModel { SubjectID = 101, YearID = 1, SemesterID = 10, DepartmentID = 50, Code = "CS 101", Title = "Intro to Programming" },
            new SubjectViewModel { SubjectID = 102, YearID = 1, SemesterID = 10, DepartmentID = 70, Code = "MATH 201", Title = "Calculus I" }
        };
        private IList<ClassCardViewModel> GetMockClasses() => new List<ClassCardViewModel> { 
            new ClassCardViewModel { ClassID = 1001, SubjectID = 101, SectionName = "A-1", Schedule = "MWF 8:00 AM", AssignedTeacher = "Dr. Smith" },
            new ClassCardViewModel { ClassID = 2001, SubjectID = 102, SectionName = "X-3", Schedule = "MWF 1:00 PM", AssignedTeacher = "Prof. Johnson" }
        };
        private IList<StudentRosterViewModel> GetMockStudents() => new List<StudentRosterViewModel> { 
            new StudentRosterViewModel { ClassID = 1001, StudentId = "S001", FullName = "Cruz, Maria L." },
            new StudentRosterViewModel { ClassID = 1001, StudentId = "S002", FullName = "Dela Rosa, Jose F." },
            new StudentRosterViewModel { ClassID = 2001, StudentId = "S005", FullName = "Lim, Kevin C." }
        };
    }
}