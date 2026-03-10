using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IApiService _api;

        public ForgotPasswordModel(IApiService api)
        {
            _api = api;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        public string Message { get; set; } = "";
        public bool ShowModal { get; set; } = false;

        public void OnGet()
        {
            // Show page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                Message = "Please enter your email address.";
                ShowModal = true;
                return Page();
            }

            var success = await _api.ForgotPasswordAsync(Email);

            Message = success
                ? "A password reset link has been sent to your email."
                : "Failed to send reset link. Please check the email address.";
            ShowModal = true;

            return Page();
        }
    }
}
