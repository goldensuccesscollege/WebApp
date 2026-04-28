using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IApiService _api;

        public ResetPasswordModel(IApiService api)
        {
            _api = api;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Token { get; set; } = "";

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public string ConfirmPassword { get; set; } = "";

        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; } = false;

        // ? New property
        public bool IsTokenValid { get; set; } = false;

        // Capture email and token from query string
        public async Task OnGetAsync(string email, string token)
        {
            Email = email;
            Token = token;

            // Check token validity via API
            IsTokenValid = await _api.CheckResetTokenAsync(email, token);
        }

        // Handle form submission
        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToPage(new { email = Email, token = Token });
            }

            var success = await _api.ResetPasswordAsync(Email, Token, NewPassword);
            if (success)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully.";
                return RedirectToPage("Login");
            }

            TempData["ErrorMessage"] = "Failed to reset password. Please try again.";
            return RedirectToPage(new { email = Email, token = Token });
        }
    }
}
