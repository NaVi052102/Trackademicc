using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Trackademic.WebApp.Pages.Student
{
    public class StudentClassModel : PageModel
    {
        // --- Properties for Dropdowns ---
        [BindProperty(SupportsGet = true)]
        public string SchoolYear { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Semester { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = string.Empty;

        // --- UPDATED SCHOOL YEAR LIST ---
        public List<SelectListItem> SchoolYears { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "2930", Text = "2930" },
            new SelectListItem { Value = "2829", Text = "2829" },
            new SelectListItem { Value = "2728", Text = "2728" },
            new SelectListItem { Value = "2627", Text = "2627" },
            new SelectListItem { Value = "2526", Text = "2526" },
            new SelectListItem { Value = "2425", Text = "2425" },
            new SelectListItem { Value = "2324", Text = "2324" },
            new SelectListItem { Value = "2223", Text = "2223" },
        };

        public List<SelectListItem> Semesters { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "First", Text = "First" },
            new SelectListItem { Value = "Second", Text = "Second" },
            new SelectListItem { Value = "Summer", Text = "Summer" },
        };

        public List<SelectListItem> Statuses { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Enrolled", Text = "Enrolled" },
            new SelectListItem { Value = "Completed", Text = "Completed" },
            new SelectListItem { Value = "Dropped", Text = "Dropped" },
        };

        // --- Data for the Class Cards ---
        public List<ClassCardViewModel> Classes { get; set; } = new List<ClassCardViewModel>();

        public void OnGet()
        {
            if (string.IsNullOrEmpty(SchoolYear)) SchoolYear = "2425";
            if (string.IsNullOrEmpty(Semester)) Semester = "First";
            if (string.IsNullOrEmpty(Status)) Status = "Enrolled";

            // --- MOCK DATA ---
            Classes = new List<ClassCardViewModel>
            {
                new ClassCardViewModel { Title = "Embedded Systems", StudentCount = 40, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Data Structures", StudentCount = 45, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Software Design", StudentCount = 38, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Technopreneurship", StudentCount = 42, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Logic Circuits", StudentCount = 40, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Data Communications", StudentCount = 37, ImageUrl = "/images/logo.png" },
                new ClassCardViewModel { Title = "Software Development 1", StudentCount = 39, ImageUrl = "/images/logo.png" }
            };
        }
    }

    public class ClassCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}