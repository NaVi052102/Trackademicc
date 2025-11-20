using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Data;
using Trackademic.Data.Models;

namespace Trackademic.WebApp.Pages.Admin.Curriculum
{
    // [Authorize(Roles = "Admin")]
    public class ClassModel : PageModel
    {
        private readonly TrackademicDbContext _context;

        public ClassModel(TrackademicDbContext context)
        {
            _context = context;
        }

        // --- Filter/Binding Properties (Using IDs) ---
        [BindProperty(SupportsGet = true)] public long SelectedYearId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedSemesterId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedDepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedSubjectId { get; set; }
        [BindProperty(SupportsGet = true)] public long SelectedClassId { get; set; }

        // --- Enrollment State ---
        [BindProperty(SupportsGet = true)]
        public bool IsEnrollmentMode { get; set; } = false;

        [BindProperty]
        public string StudentSearchTerm { get; set; }

        // --- Dropdowns ---
        public List<SelectListItem> YearOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SemesterOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubjectOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ClassOptions { get; set; } = new List<SelectListItem>();

        // --- View Data ---
        public ClassDetailsViewModel SelectedClassDetails { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
        public List<StudentRosterViewModel> EnrollmentSearchResults { get; set; } = new List<StudentRosterViewModel>();

        public async Task OnGetAsync()
        {
            await LoadAllDataAsync();
        }

        // --- HANDLERS ---

        public async Task<IActionResult> OnPostEnrollModeAsync()
        {
            await LoadAllDataAsync();
            IsEnrollmentMode = true;
            return Page();
        }

        public async Task<IActionResult> OnPostSearchStudentAsync()
        {
            await LoadAllDataAsync();
            IsEnrollmentMode = true;

            if (!string.IsNullOrEmpty(StudentSearchTerm))
            {
                string term = StudentSearchTerm.ToLower();

                // Search in Students table
                var matches = await _context.Students
                    .Where(s => s.StudentNumber.ToLower().Contains(term)
                             || s.FirstName.ToLower().Contains(term)
                             || s.LastName.ToLower().Contains(term))
                    .Take(10) // Limit results
                    .Select(s => new StudentRosterViewModel
                    {
                        StudentId = s.StudentNumber,
                        FullName = $"{s.LastName}, {s.FirstName}"
                    })
                    .ToListAsync();

                EnrollmentSearchResults = matches;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostEnrollStudentAsync(string studentIdToEnroll)
        {
            if (SelectedClassId == 0) return RedirectToPage(GetRouteData());

            // 1. Find Student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentIdToEnroll);
            if (student == null)
            {
                TempData["Message"] = "Student not found.";
                return RedirectToPage(GetRouteData());
            }

            // 2. Check if already enrolled
            bool isEnrolled = await _context.Classenrollments
                .AnyAsync(e => e.ClassId == SelectedClassId && e.StudentId == student.Id);

            if (isEnrolled)
            {
                TempData["Message"] = "Student is already enrolled in this class.";
            }
            else
            {
                // 3. Enroll
                var enrollment = new Classenrollment
                {
                    ClassId = SelectedClassId,
                    StudentId = student.Id,
                    // FIX: Convert DateTime to DateOnly
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                    EnrollmentStatus = "Enrolled"
                };
                _context.Classenrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Student {student.FirstName} {student.LastName} enrolled successfully.";
            }

            return RedirectToPage(GetRouteData());
        }

        public async Task<IActionResult> OnPostRemoveStudentAsync(string studentId)
        {
            if (SelectedClassId == 0) return RedirectToPage(GetRouteData());

            // 1. Find Student internal ID from String ID
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentId);
            if (student == null) return RedirectToPage(GetRouteData());

            // 2. Find Enrollment Record
            var enrollment = await _context.Classenrollments
                .FirstOrDefaultAsync(e => e.ClassId == SelectedClassId && e.StudentId == student.Id);

            if (enrollment != null)
            {
                // 3. Remove
                // Optional: Check for grades
                var hasGrades = await _context.Grades.AnyAsync(g => g.EnrollmentId == enrollment.Id);
                if (hasGrades)
                {
                    // If you want to allow soft delete or status change instead:
                    enrollment.EnrollmentStatus = "Dropped";
                    _context.Classenrollments.Update(enrollment);
                    TempData["Message"] = "Student marked as Dropped (Grades preserved).";
                }
                else
                {
                    // Hard delete if no grades
                    _context.Classenrollments.Remove(enrollment);
                    TempData["Message"] = "Student removed from class roster.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(GetRouteData());
        }

        // --- LOADING HELPERS ---
        private async Task LoadAllDataAsync()
        {
            // 1. Load Years
            YearOptions = await _context.Schoolyears
                .OrderByDescending(y => y.Id)
                .Select(y => new SelectListItem { Value = y.Id.ToString(), Text = y.YearName, Selected = y.Id == SelectedYearId })
                .ToListAsync();

            // 2. Load Semesters (Dependent on Year)
            if (SelectedYearId > 0)
            {
                SemesterOptions = await _context.Semesters
                    .Where(s => s.SchoolYearId == SelectedYearId)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.SemesterName, Selected = s.Id == SelectedSemesterId })
                    .ToListAsync();
            }

            // 3. Load Departments
            DepartmentOptions = await _context.Departments
                .OrderBy(d => d.DeptName)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.DeptName, Selected = d.Id == SelectedDepartmentId })
                .ToListAsync();

            // 4. Load Subjects (Dependent on Department)
            if (SelectedDepartmentId > 0)
            {
                SubjectOptions = await _context.Subjects
                    .Where(s => s.DepartmentId == SelectedDepartmentId)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.SubjectCode} - {s.SubjectName}", Selected = s.Id == SelectedSubjectId })
                    .ToListAsync();
            }

            // 5. Load Classes (Dependent on Year, Sem, Subject)
            if (SelectedYearId > 0 && SelectedSemesterId > 0 && SelectedSubjectId > 0)
            {
                ClassOptions = await _context.Classes
                    .Where(c => c.SchoolYearId == SelectedYearId && c.SemesterId == SelectedSemesterId && c.SubjectId == SelectedSubjectId)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ClassSection, Selected = c.Id == SelectedClassId })
                    .ToListAsync();
            }

            // 6. Load Details & Roster (Only if a class is selected)
            if (SelectedClassId > 0)
            {
                await LoadClassDetailsAsync(SelectedClassId);
                await LoadStudentRosterAsync(SelectedClassId);
            }
        }

        private async Task LoadClassDetailsAsync(long classId)
        {
            var classData = await _context.Classes
                .Include(c => c.Subject)
                .Include(c => c.Classassignments)
                    .ThenInclude(ca => ca.Teacher)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classData != null)
            {
                int count = await _context.Classenrollments.CountAsync(e => e.ClassId == classId);
                var assignment = classData.Classassignments.FirstOrDefault();
                string teacherName = assignment != null ? $"{assignment.Teacher.FirstName} {assignment.Teacher.LastName}" : "Unassigned";

                SelectedClassDetails = new ClassDetailsViewModel
                {
                    SubjectCode = classData.Subject.SubjectCode,
                    SectionName = classData.ClassSection,
                    CourseTitle = classData.Subject.SubjectName,
                    TotalStudents = count,
                    AssignedTeacher = teacherName
                };
            }
        }

        private async Task LoadStudentRosterAsync(long classId)
        {
            Students = await _context.Classenrollments
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && e.EnrollmentStatus == "Enrolled") // Only show active
                .OrderBy(e => e.Student.LastName)
                .Select(e => new StudentRosterViewModel
                {
                    ClassId = e.ClassId,
                    StudentId = e.Student.StudentNumber,
                    FullName = $"{e.Student.LastName}, {e.Student.FirstName}"
                })
                .ToListAsync();
        }

        private object GetRouteData() => new { SelectedYearId, SelectedSemesterId, SelectedDepartmentId, SelectedSubjectId, SelectedClassId };

        // --- VIEW MODELS ---
        public class ClassDetailsViewModel
        {
            public string SubjectCode { get; set; }
            public string SectionName { get; set; }
            public string CourseTitle { get; set; }
            public int TotalStudents { get; set; }
            public string AssignedTeacher { get; set; }
        }

        public class StudentRosterViewModel
        {
            public long ClassId { get; set; }
            public string StudentId { get; set; }
            public string FullName { get; set; }
        }
    }
}