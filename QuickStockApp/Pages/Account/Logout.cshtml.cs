using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuickStockApp.Pages.Account
{
    [Authorize] // Only logged-in users can logout
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            // ✅ Sign out cookie
            await HttpContext.SignOutAsync("MyCookieAuth");

            // Redirect to login page
            return RedirectToPage("/Account/Login");
        }
    }
}
