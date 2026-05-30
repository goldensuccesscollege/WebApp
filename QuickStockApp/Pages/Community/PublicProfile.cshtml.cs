using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages.Community
{
    public class PublicProfileModel : PageModel
    {
        private readonly IApiService _api;
        private readonly IConfiguration _config;

        public PublicProfileModel(IApiService api, IConfiguration config)
        {
            _api = api;
            _config = config;
            ApiBaseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        }

        public string? ApiBaseUrl { get; }
        public ProfileDto? PublicProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Index");

            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Account/Login");

            PublicProfile = await _api.GetPublicProfileAsync(token, username);
            if (PublicProfile == null)
            {
                ViewData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            return Page();
        }
    }
}
