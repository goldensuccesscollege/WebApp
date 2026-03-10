using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Security.Claims;

namespace QuickStockApp.Pages.Account
{
    [AllowAnonymous] // Allow unauthenticated users
    public class LoginModel : PageModel
    {
        private readonly IApiService _apiService;

        public LoginModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public LoginRequestDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? Message { get; set; }
        public bool IsSuccess { get; set; } = true;

        public IActionResult OnGet()
        {
            // If already logged in, redirect to Dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Dashboard/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Username) || string.IsNullOrWhiteSpace(Input.Password))
            {
                Message = "Username and password are required.";
                IsSuccess = false;
                return Page();
            }

            var (result, errorMessage) = await _apiService.LoginAsync(Input);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                Message = errorMessage ?? "Invalid username or password.";
                IsSuccess = false;
                return Page();
            }

            // ✅ Create cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Input.Username),
                new Claim(ClaimTypes.Role, result.Role ?? "User"),
                new Claim("jwt_token", result.Token)
            };
            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            // ✅ Redirect to ReturnUrl if present
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            // Default redirect
            return RedirectToPage("/Dashboard/Index");
        }
    }
}
