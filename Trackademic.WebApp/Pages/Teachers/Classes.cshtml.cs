using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Teachers
{
    public class ClassesModel : PageModel
    {
        // --- Filter/Binding Properties ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ClassId { get; set; }

        // --- Enrollment State Properties ---
        [BindProperty(SupportsGet = true)]
        public bool IsEnrollmentMode { get; set; } = false; 

        [BindProperty]
        public string StudentSearchTerm { get; set; }
        
        // --- Dropdown Population Lists ---
        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2930", Text = "2930" },
            new SelectListItem { Value = "2829", Text = "2829" },
            new SelectListItem { Value = "2425", Text = "2425" }
        };

        public List<SelectListItem> Semesters { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "First", Text = "First" },
            new SelectListItem { Value = "Second", Text = "Second" },
            new SelectListItem { Value = "Summer", Text = "Summer" }
        };

        public List<SelectListItem> ClassSelectList { get; set; }

        // --- Detail View Data ---
        public ClassDetailsViewModel SelectedClass { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
        
        // Data container for the search results in enrollment mode
        public List<StudentRosterViewModel> EnrollmentSearchResults { get; set; } = new List<StudentRosterViewModel>();


        public void OnGet()
        {
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";

            LoadAllClassesForDropdown(SchoolYear, Semester);

            if (!string.IsNullOrEmpty(ClassId))
            {
                LoadClassDetails(ClassId, SchoolYear, Semester);
                LoadStudentRoster(ClassId, SchoolYear, Semester);
            }
        }
        
        // Handler to switch the UI to Enrollment Mode
        public IActionResult OnPostEnrollMode()
        {
            LoadAllClassesForDropdown(SchoolYear, Semester);
            LoadClassDetails(ClassId, SchoolYear, Semester);
            LoadStudentRoster(ClassId, SchoolYear, Semester);
            IsEnrollmentMode = true; 
            return Page();
        }

        // Handler to search for a student when in Enrollment Mode
        public IActionResult OnPostSearchStudent()
        {
            LoadAllClassesForDropdown(SchoolYear, Semester);
            LoadClassDetails(ClassId, SchoolYear, Semester);
            LoadStudentRoster(ClassId, SchoolYear, Semester); 
            IsEnrollmentMode = true;

            if (!string.IsNullOrEmpty(StudentSearchTerm))
            {
                string term = StudentSearchTerm.ToLower();

                // --- SIMULATED SEARCH LOGIC (UPDATED WITH FIRST/LAST NAMES) ---
                
                if (term.Contains("gomez"))
                {
                    EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "S006", LastName = "Gomez", FirstName = "Mark D." });
                }
                else if (term.Contains("t001") || term.Contains("tan"))
                {
                    EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "T001", LastName = "Tan", FirstName = "Lily A." });
                    // Added Tamayo here as well if searching generally
                    EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "T002", LastName = "Tamayo", FirstName = "Ben R." });
                }
                // NEW: Added logic to find a new student
                else if (term.Contains("rivera") || term.Contains("josh")) 
                {
                    EnrollmentSearchResults.Add(new StudentRosterViewModel { StudentId = "N099", LastName = "Rivera", FirstName = "Josh" });
                }
            }
            return Page();
        }
        
        // NEW HANDLER: Remove a student from the class
        public IActionResult OnPostRemoveStudent(string studentIdToRemove)
        {
            // NOTE: This currently relies on static data, so the student will reappear on next load.
            TempData["Message"] = $"Student ID {studentIdToRemove} successfully removed (simulated).";
            TempData["MessageType"] = "success"; 
            
            return RedirectToPage(new { SchoolYear, Semester, ClassId });
        }
        
        // Placeholder for the actual enrollment action
        public IActionResult OnPostEnrollStudent(string studentIdToEnroll)
        {
            // In a real app, you would execute the INSERT into classenrollment table here.
            TempData["Message"] = $"Student ID {studentIdToEnroll} successfully enrolled (simulated).";
            TempData["MessageType"] = "success"; 
            
            return RedirectToPage(new { SchoolYear, Semester, ClassId, IsEnrollmentMode = false });
        }


        // --- Private Data Loading Methods ---
        private void LoadAllClassesForDropdown(string year, string semester)
        {
            var allClasses = new List<ClassCardViewModel>();
            if (year == "2425" && semester == "First")
            {
                allClasses.Add(new ClassCardViewModel { ClassId = "CPE461-H2", Title = "CPE461 - Embedded Systems (H2-4R4)" });
                allClasses.Add(new ClassCardViewModel { ClassId = "CS201-B1", Title = "CS201 - Data Structures (B1-3T2)" });
                allClasses.Add(new ClassCardViewModel { ClassId = "SE305-A3", Title = "SE305 - Software Design (A3-2A1)" });
            }
            ClassSelectList = allClasses.Select(c => new SelectListItem { Value = c.ClassId, Text = c.Title }).ToList();
        }

        private void LoadClassDetails(string classId, string year, string semester)
        {
            if (year == "2425" && semester == "First" && classId == "CPE461-H2")
            {
                SelectedClass = new ClassDetailsViewModel { SubjectCode = "CPE461", SectionName = "H2-4R4", CourseTitle = "Embedded Systems", TotalStudents = 32 };
            }
            else if (classId == "CS201-B1")
            {
                SelectedClass = new ClassDetailsViewModel { SubjectCode = "CS201", SectionName = "B1-3T2", CourseTitle = "Data Structures", TotalStudents = 45 };
            }
        }

        private void LoadStudentRoster(string classId, string year, string semester)
        {
            if (year == "2425" && semester == "First" && classId == "CPE461-H2")
            {
                // UPDATED: Populating First Name and Last Name separately
                Students = new List<StudentRosterViewModel>
                {
                    new StudentRosterViewModel { StudentId = "S001", LastName = "Cruz", FirstName = "Maria L." },
                    new StudentRosterViewModel { StudentId = "S002", LastName = "Dela Rosa", FirstName = "Jose F." },
                    new StudentRosterViewModel { StudentId = "S003", LastName = "Reyes", FirstName = "Miguel T." },
                    new StudentRosterViewModel { StudentId = "S004", LastName = "Santos", FirstName = "Anna K." },
                    new StudentRosterViewModel { StudentId = "S005", LastName = "Lim", FirstName = "Kevin C." }
                };
            }
        }
    }

    // --- View Models ---
    public class ClassCardViewModel { public string ClassId { get; set; } public string Title { get; set; } }
    public class ClassDetailsViewModel { public string SubjectCode { get; set; } public string SectionName { get; set; } public string CourseTitle { get; set; } public int TotalStudents { get; set; } }
    
    // UPDATED VIEW MODEL
    public class StudentRosterViewModel 
    { 
        public string StudentId { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        
        // Helper property: This allows your HTML (@student.FullName) to keep working!
        public string FullName => $"{LastName}, {FirstName}"; 
    }
}