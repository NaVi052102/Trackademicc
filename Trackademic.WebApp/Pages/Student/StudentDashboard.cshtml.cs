using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System;
using Trackademic.Core.Interfaces; // Assuming this namespace exists
using Trackademic.Core.Models;      // Assuming this namespace exists

namespace Trackademic.Pages.Student
{
    // Class definition for the Enrolled Classes list
    public class ClassEnrollmentDisplay
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int Units { get; set; } = 0;
        public string ImageUrl { get; set; } = "/images/logo.png";
    }

    // REQUIRED: Class definition for current performance display
    public class PerformanceDisplay
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentGrade { get; set; }
        public int Percentage { get; set; } = 0;
    }

    public class StudentDashboardModel : PageModel
    {
        private readonly IGradeService _gradeService;

        public StudentDashboardModel(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        // --- PROPERTIES REQUIRED BY THE RAZOR VIEW ---

        public decimal CumulativeGPA { get; set; } = 0.0m;
        public int TotalUnitsEarned { get; set; } = 0;
        public int PassedSubjects { get; set; } = 0;
        public int TotalSubjects { get; set; } = 0;

        public List<string> PerformanceAlerts { get; set; } = new();
        public Dictionary<string, decimal> GpaHistory { get; set; } = new();
        public List<ClassEnrollmentDisplay> EnrolledClasses { get; set; } = new();

        // REQUIRED PROPERTY
        public List<PerformanceDisplay> PerformanceData { get; set; } = new();

        // --- PAGE HANDLER AND LOGIC ---

        public void OnGet()
        {
            LoadCurrentTermClasses();
            LoadGpaHistory();
            CalculateCumulativeStatistics();
            GeneratePerformanceAlerts();
            GenerateCurrentPerformanceData();
        }

        private void GenerateCurrentPerformanceData()
        {
            // MOCK data for current semester grades (1.0 - 5.0 scale)
            var currentGrades = new List<PerformanceDisplay>
            {
                new PerformanceDisplay { SubjectCode = "CPE331", Description = "Data and Digital Communications", CurrentGrade = 4.5m },
                new PerformanceDisplay { SubjectCode = "CPE361", Description = "Logic Circuits and Design", CurrentGrade = 3.5m },
                new PerformanceDisplay { SubjectCode = "ES038", Description = "Technopreneurship", CurrentGrade = 2.0m },
                new PerformanceDisplay { SubjectCode = "CPE333", Description = "Basic Safety and Health", CurrentGrade = 1.5m },
                new PerformanceDisplay { SubjectCode = "CPE335", Description = "Feedback and Control Systems", CurrentGrade = 4.0m },
            };

            foreach (var perf in currentGrades)
            {
                // Formula: Percentage = ((Grade - 1.0) / 4.0) * 100 
                decimal percentage = ((perf.CurrentGrade - 1.0m) / 4.0m) * 100m;

                perf.Percentage = (int)Math.Clamp(Math.Round(percentage), 0, 100);

                PerformanceData.Add(perf);
            }
        }

        private void LoadCurrentTermClasses()
        {
            // MOCK DATA for Enrolled Classes 
            EnrolledClasses = new List<ClassEnrollmentDisplay>
            {
                new ClassEnrollmentDisplay { SubjectCode = "CPE331", Description = "Data and Digital Communications", FacultyName = "SEMBLANTE, J. N.", Units = 3 },
                new ClassEnrollmentDisplay { SubjectCode = "CPE361", Description = "Logic Circuits and Design", FacultyName = "CORTES, S. G.", Units = 4 },
                new ClassEnrollmentDisplay { SubjectCode = "ES038", Description = "Technopreneurship", FacultyName = "BARRIOQUINTO, E. M.", Units = 3 },
                new ClassEnrollmentDisplay { SubjectCode = "CPE333", Description = "Basic Safety and Health", FacultyName = "ALFEREZ, N.", Units = 3 },
                new ClassEnrollmentDisplay { SubjectCode = "CPE335", Description = "Feedback and Control Systems", FacultyName = "TAMPUS, M. J.", Units = 3 },
            };
        }

        private void CalculateCumulativeStatistics()
        {
            // MOCK Cumulative Data
            CumulativeGPA = 4.12m;
            TotalUnitsEarned = 78;
            PassedSubjects = 18;
            TotalSubjects = 20;
        }


        private void LoadGpaHistory()
        {
            // UPDATED MOCK GPA HISTORY: Uses School Year and Semester Number
            GpaHistory = new Dictionary<string, decimal>
            {
                { "2022-2023 - 2nd Semester", 3.75m },
                { "2023-2024 - 1st Semester", 3.90m },
                { "2023-2024 - 2nd Semester", 4.05m },
                { "2024-2025 - 1st Semester", 4.12m }
            };
        }

        private void GeneratePerformanceAlerts()
        {
            // MOCK Alert Generation logic
            var historicalGrades = new List<(string code, string status)>
            {
                ("CPE335", "FAILED"),
                ("CPE101", "INC"),
                ("MAT201", "FAILED"),
            };

            foreach (var grade in historicalGrades)
            {
                if (grade.status == "FAILED")
                {
                    PerformanceAlerts.Add($"<i class='bi bi-x-octagon-fill text-danger me-1'></i> **FAILED:** Subject {grade.code} (Action required).");
                }
                else if (grade.status == "INC")
                {
                    PerformanceAlerts.Add($"<i class='bi bi-exclamation-triangle-fill text-warning me-1'></i> **INCOMPLETE:** Subject {grade.code} (Submit pending requirements).");
                }
            }
        }
    }
}