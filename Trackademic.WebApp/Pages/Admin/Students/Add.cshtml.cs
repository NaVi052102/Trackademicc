using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Trackademic.Pages.Admin.Students
{
    // This tag allows you to view the page without being logged in
    [AllowAnonymous]
    public class AddModel : PageModel
    {
        // This method runs when the page is first loaded (using GET)
        public void OnGet()
        {
        }

        // This method will run when you click the "Create Student Account" button (using POST)
        public void OnPost()
        {
            // We will add code here later to save the form data
        }
    }
}