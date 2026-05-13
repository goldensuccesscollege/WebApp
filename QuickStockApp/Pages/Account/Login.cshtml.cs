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
                TempData["ErrorMessage"] = errorMessage ?? "Invalid username or password.";
                return RedirectToPage();
            }

            // ✅ Create cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
                new Claim(ClaimTypes.Name, Input.Username),

                new Claim(ClaimTypes.Role, result.Role ?? "User"),
                new Claim("jwt_token", result.Token),
                new Claim("CanAccessITAssets", result.CanAccessITAssets.ToString()),
                new Claim("CanAccessApparel", result.CanAccessApparel.ToString()),
                new Claim("CanAccessMessages", result.CanAccessMessages.ToString()),
                new Claim("CanAccessLibrary", result.CanAccessLibrary.ToString()),
                new Claim("CanAccessHomeEconomics", result.CanAccessHomeEconomics.ToString()),
                new Claim("CanAccessConsumables", result.CanAccessConsumables.ToString())
            };

            if (result.CampusIds != null && result.CampusIds.Any())
            {
                foreach (var cid in result.CampusIds)
                {
                    claims.Add(new Claim("CampusId", cid.ToString()));
                }

                // AI: Auto-assign active campus if only one is assigned
                if (result.CampusIds.Count == 1)
                {
                    var campusId = result.CampusIds[0];
                    claims.Add(new Claim("ActiveCampusId", campusId.ToString()));
                    
                    // Fetch name for the claim
                    var allCampuses = await _apiService.GetCampusesAsync(result.Token);
                    var currentCampus = allCampuses.FirstOrDefault(c => c.CampusId == campusId);
                    if (currentCampus != null)
                    {
                        claims.Add(new Claim("ActiveCampusName", currentCampus.Name));
                    }
                }
            }
            else if (!"Admin".Equals(result.Role, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Access Denied: Your account is not assigned to any campus. Please contact the administrator.";
                return RedirectToPage();
            }

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            // ✅ Redirect to ReturnUrl if present
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            if ("Admin".Equals(result.Role, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Campus/Campuses");
            }

            if (result.CampusIds != null && result.CampusIds.Count == 1)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            return RedirectToPage("/Campus/Campuses");
        }

        public async Task<IActionResult> OnPostVerifyTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new JsonResult(new { success = false, message = "Token is required." });
            }

            var result = await _apiService.VerifyAsync(token);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }
    }
}
