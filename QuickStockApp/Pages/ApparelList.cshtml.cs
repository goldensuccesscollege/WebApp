using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;
using QuickStockApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuickStockApp.Pages
{
    [Authorize]
    public class ApparelListModel : PageModel
    {
        private readonly IApiService _apiService;

        public ApparelListModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<ApparelDto> Apparel { get; set; } = new();
        public List<CampusDto> AssignedCampuses { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public ApparelDto ApparelItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Permission check
            var canAccess = User.IsInRole("Admin") || (User.FindFirst("CanAccessApparel")?.Value == "True");
            if (!canAccess)
            {
                return Forbid();
            }

            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;

            if (User.IsInRole("Admin"))
            {
                if (int.TryParse(activeCampusClaim, out int acid))
                {
                    filterCampusId = acid;
                }
                else
                {
                    return RedirectToPage("/Campuses");
                }
            }
            else
            {
                var userCampusIdStr = User.FindFirst("CampusId")?.Value;
                if (int.TryParse(userCampusIdStr, out int userCid))
                {
                    filterCampusId = userCid;
                }
            }

            Apparel = await _apiService.GetApparelAsync(filterCampusId, SearchTerm);

            var allCampuses = await _apiService.GetCampusesAsync();
            if (User.IsInRole("Admin"))
            {
                AssignedCampuses = allCampuses;
            }
            else
            {
                var userIds = User.FindAll("CampusId").Select(c => int.Parse(c.Value)).ToList();
                AssignedCampuses = allCampuses.Where(c => userIds.Contains(c.CampusId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (ApparelItem.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    ApparelItem.CampusId = acid;
                }
            }

            var result = await _apiService.AddApparelAsync(ApparelItem);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Apparel added successfully!";
                return RedirectToPage();
            }
            
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var result = await _apiService.UpdateApparelAsync(ApparelItem);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Apparel updated successfully!";
                return RedirectToPage();
            }
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _apiService.DeleteApparelAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Apparel deleted successfully!";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetItemsAsync(int id)
        {
            var items = await _apiService.GetApparelItemsAsync(id);
            return new JsonResult(items);
        }
    }
}
