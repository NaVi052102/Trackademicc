using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
// --- FIXES ---
using Trackademic.Core.Interfaces; // FIX: IGradeService location
using Trackademic.Core.Models;    // FIX: Models location
// --- END FIXES ---

namespace Trackademic.Pages.Student
{
    public class StudentViewGradeModel : PageModel
    {
        private readonly IGradeService _gradeService;

        public StudentViewGradeModel(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        // Grade display class
        public class GradeDisplay
        {
            public int Id { get; set; }
            public string SubjectCode { get; set; } = string.Empty; // Initialized
            public string Description { get; set; } = string.Empty; // Initialized
            public string FacultyName { get; set; } = string.Empty; // Initialized
            public int Units { get; set; }
            public string Midterm { get; set; } = string.Empty;
            public string Final { get; set; } = string.Empty;
            public decimal FinalGrade { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        // --- PROPERTIES INITIALIZED TO PREVENT WARNINGS ---
        public List<GradeDisplay> Grades { get; set; } = new();
        public decimal GPA { get; set; }
        public int TotalUnits { get; set; }
        public int PassedSubjects { get; set; }
        public int FailedSubjects { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string SelectedTermDisplay { get; set; } = string.Empty;

        // Dropdowns
        public List<SelectListItem> SchoolYears { get; set; } = new();
        public List<SelectListItem> Semesters { get; set; } = new();
        // --- END PROPERTY INITIALIZATION ---

        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = "2024-2025";

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = "1st Semester";

        public void OnGet()
        {
            LoadGrades();
            LoadDropdowns();
        }

        private void LoadGrades()
        {
            // --- MOCK DATA ---
            // This bypasses the service call to ensure the UI loads.
            Grades = new List<GradeDisplay>
            {
                new GradeDisplay { Id = 1, SubjectCode = "CPE331", Description = "Data and Digital Communications", FacultyName = "SEMBLANTE, JULIAN N", Units = 3, Midterm = "4.4", Final = "4.4", FinalGrade = 4.4m, Status = "PASSED" },
                new GradeDisplay { Id = 2, SubjectCode = "CPE333", Description = "Basic Occupational Safety and Health", FacultyName = "ALFEREZ, NIKKO", Units = 3, Midterm = "4.7", Final = "4.7", FinalGrade = 4.7m, Status = "PASSED" },
                new GradeDisplay { Id = 3, SubjectCode = "CPE335", Description = "Feedback and Control Systems", FacultyName = "TAMPUS, MERVIN JOHN C", Units = 3, Midterm = "INC", Final = "5.0", FinalGrade = 5.0m, Status = "PASSED" },
                new GradeDisplay { Id = 4, SubjectCode = "CPE361", Description = "Logic Circuits and Design", FacultyName = "CORTES, STEPHANie GRACE VILLARUBIA", Units = 4, Midterm = "4.6", Final = "4.7", FinalGrade = 4.7m, Status = "PASSED" },
                new GradeDisplay { Id = 5, SubjectCode = "CPE363", Description = "Software Design", FacultyName = "CORTES, STEPHANIE GRACE VILLARUBIA", Units = 4, Midterm = "4.4", Final = "4.4", FinalGrade = 4.4m, Status = "PASSED" },
                new GradeDisplay { Id = 6, SubjectCode = "CPE381", Description = "Computer Engineering Drafting and Design", FacultyName = "Alterado, Jundith Degala", Units = 1, Midterm = "4.0", Final = "4.7", FinalGrade = 4.7m, Status = "PASSED" },
                new GradeDisplay { Id = 7, SubjectCode = "CPEPE361SD", Description = "Software Development 1", FacultyName = "Bultawe, Jovelyn Banguis", Units = 3, Midterm = "4.5", Final = "4.7", FinalGrade = 4.7m, Status = "PASSED" },
                new GradeDisplay { Id = 8, SubjectCode = "ES038", Description = "Technopreneurship", FacultyName = "BARRIOQUINTO, ELLA MARIE Y", Units = 3, Midterm = "INC", Final = "4.6", FinalGrade = 4.6m, Status = "PASSED" }
            };
            // --- END OF MOCK DATA ---

            CalculateStatistics();

            ProgramName = "Bachelor of Science in Information Technology";
            SelectedTermDisplay = $"{SchoolYear} - {Semester}";
        }

        private void CalculateStatistics()
        {
            if (Grades == null || !Grades.Any())
            {
                GPA = 0;
                TotalUnits = 0;
                PassedSubjects = 0;
                FailedSubjects = 0;
                return;
            }

            decimal totalGradePoints = 0;
            TotalUnits = 0;
            var gradedEntries = Grades.Where(g => g.Status != "INC" && g.Status != "N/A");

            foreach (var grade in gradedEntries)
            {
                totalGradePoints += grade.FinalGrade * grade.Units;
                TotalUnits += grade.Units;
            }

            GPA = TotalUnits > 0 ? totalGradePoints / TotalUnits : 0;
            PassedSubjects = Grades.Count(g => g.Status == "PASSED");
            FailedSubjects = Grades.Count(g => g.Status == "FAILED");
        }

        private void LoadDropdowns()
        {
            SchoolYears = new List<SelectListItem>
            {
                new SelectListItem { Value = "2029-2030", Text = "2029-2030" },
                new SelectListItem { Value = "2028-2029", Text = "2028-2029" },
                new SelectListItem { Value = "2027-2028", Text = "2027-2028" },
                new SelectListItem { Value = "2026-2027", Text = "2026-2027" },
                new SelectListItem { Value = "2025-2026", Text = "2025-2026" },
                new SelectListItem { Value = "2024-2025", Text = "2024-2025" },
                new SelectListItem { Value = "2023-2024", Text = "2023-2024" },
                new SelectListItem { Value = "2022-2023", Text = "2022-2023" }
            };

            Semesters = new List<SelectListItem>
            {
                new SelectListItem { Value = "1st Semester", Text = "1st Semester" },
                new SelectListItem { Value = "2nd Semester", Text = "2nd Semester" },
                new SelectListItem { Value = "Summer", Text = "Summer" }
            };
        }
    }
}