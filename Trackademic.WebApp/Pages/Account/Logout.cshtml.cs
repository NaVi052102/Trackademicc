using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Trackademic.WebApp.Pages.Account
{
    public class LogoutModel : PageModel
    {
        // We use OnPost because changing authentication state should always be a POST request
        // for security reasons (prevents Cross-Site Request Forgery).
        public async Task<IActionResult> OnPostAsync()
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear the Session to ensure no data lingers
            HttpContext.Session.Clear();

            // Redirect user back to the Login page
            return RedirectToPage("/Account/Login");
        }
    }
}