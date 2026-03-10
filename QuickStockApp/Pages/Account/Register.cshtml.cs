using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    [AllowAnonymous]
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
                ModalMessage = "Passwords do not match.";
                ShowModal = true;
                IsSuccess = false;
                return Page();
            }

            // Call API
            Register.IsFromApi = true;
            var result = await _api.RegisterAsync(Register);

            ModalMessage = result.Success ? "Register successfully" : "Registration failed";
            ShowModal = true;
            IsSuccess = result.Success;

            return Page();
        }
    }
}
