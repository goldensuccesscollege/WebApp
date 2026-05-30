using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using Microsoft.Extensions.Configuration;

namespace QuickStockApp.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IApiService _api;
        private readonly IConfiguration _config;

        public ProfileModel(IApiService api, IConfiguration config)
        {
            _api = api;
            _config = config;
            ApiBaseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        }
        
        public string? ApiBaseUrl { get; }

        [BindProperty]
        public UpdateProfileRequest ProfileRequest { get; set; } = new();

        public ProfileDto? CurrentProfile { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            CurrentProfile = await _api.GetProfileAsync(token);
            if (CurrentProfile == null)
            {
                ErrorMessage = "Could not load profile.";
                return Page();
            }

            // Map to request object
            ProfileRequest.FirstName = CurrentProfile.FirstName;
            ProfileRequest.LastName = CurrentProfile.LastName;
            ProfileRequest.Birthday = CurrentProfile.Birthday;
            ProfileRequest.Address = CurrentProfile.Address;
            ProfileRequest.PhoneNumber = CurrentProfile.PhoneNumber;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(IFormFile? ImageProfile)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            Stream? imageStream = null;
            string? fileName = null;

            if (ImageProfile != null)
            {
                imageStream = ImageProfile.OpenReadStream();
                fileName = ImageProfile.FileName;
            }

            var (success, message) = await _api.UpdateProfileAsync(token, ProfileRequest, imageStream, fileName);

            if (success) TempData["SuccessMessage"] = "Profile updated successfully!";
            else TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }
    }
}
