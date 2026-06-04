using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace QuickStockApp.Pages.Campus
{
    public class CampusesModel : PageModel
    {
        private readonly IApiService _apiService;

        public CampusesModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<CampusDto> Campuses { get; set; } = new();

        [BindProperty]
        public CampusDto Campus { get; set; } = new();

        public string UserRole { get; set; } = "Employee";
        public int? AssignedCampusId { get; set; }

        public async Task OnGetAsync()
        {
            UserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
            Campuses = await _apiService.GetCampusesAsync();

            // If regular user is assigned to specific campuses, show only those
            if (UserRole != "Admin")
            {
                var assignedCampusIds = User.FindAll("CampusId").Select(c => c.Value).ToList();
                if (assignedCampusIds.Any())
                {
                    Campuses = Campuses.Where(c => assignedCampusIds.Contains(c.CampusId.ToString())).ToList();
                }
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, message) = await _apiService.AddCampusAsync(Campus);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Campus created successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid || Campus.CampusId <= 0)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, message) = await _apiService.UpdateCampusAsync(Campus);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Campus updated successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (success, message) = await _apiService.DeleteCampusAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Campus deleted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSelectCampusAsync(int id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null) return RedirectToPage("/Account/Login");

            // Remove existing ActiveCampusId if any
            var existingClaim = identity.FindFirst("ActiveCampusId");
            if (existingClaim != null) identity.RemoveClaim(existingClaim);

            // Add new ActiveCampusId
            identity.AddClaim(new Claim("ActiveCampusId", id.ToString()));

            // Extract the campus name for display in layout if possible, or just the ID
            var campus = (await _apiService.GetCampusesAsync()).FirstOrDefault(c => c.CampusId == id);
            if (campus != null)
            {
                var nameClaim = identity.FindFirst("ActiveCampusName");
                if (nameClaim != null) identity.RemoveClaim(nameClaim);
                identity.AddClaim(new Claim("ActiveCampusName", campus.Name));
            }

            // Re-sign in to refresh cookie
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
