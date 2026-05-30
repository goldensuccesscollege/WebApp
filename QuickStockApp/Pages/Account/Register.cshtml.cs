using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    [Authorize(Roles = "Admin")]
    public class RegisterModel : PageModel
    {
        private readonly IApiService _api;

        public RegisterModel(IApiService api)
        {
            _api = api;
        }

        [BindProperty]
        public RegisterRequest Register { get; set; } = new();

        public string ModalMessage { get; set; } = "";
        public bool ShowModal { get; set; } = false;
        public bool IsSuccess { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {
            // Password match validation
            if (Register.Password != Register.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToPage();
            }

            // Call API
            Register.IsFromApi = true;
            var result = await _api.RegisterAsync(Register);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Account created successfully!";
                return RedirectToPage();
            }
            
            TempData["ErrorMessage"] = result.Message ?? "Registration failed. Please try again.";
            return RedirectToPage();
        }
    }
}
