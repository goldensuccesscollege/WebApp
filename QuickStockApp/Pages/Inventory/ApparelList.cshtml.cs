using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;
using QuickStockApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuickStockApp.Pages.Inventory
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

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        [BindProperty]
        public ApparelDto ApparelItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Permission check
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            var canAccess = isAnyAdmin || (User.FindFirst("CanAccessApparel")?.Value == "True");
            if (!canAccess)
            {
                return Forbid();
            }

            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;

            if (int.TryParse(activeCampusClaim, out int acid))
            {
                filterCampusId = acid;
            }
            else
            {
                return RedirectToPage("/Campus/Campuses");
            }

            var paginatedResult = await _apiService.GetApparelAsync(filterCampusId, SearchTerm, CurrentPage, 5);
            Apparel = paginatedResult.Apparel;
            TotalPages = paginatedResult.TotalPages;
            TotalItems = paginatedResult.TotalItems;

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
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager") && !User.IsInRole("User")) return Forbid();

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
                return RedirectToPage(new { showList = true });
            }
            
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager")) return Forbid();

            var result = await _apiService.UpdateApparelAsync(ApparelItem);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Apparel updated successfully!";
                return RedirectToPage(new { showList = true });
            }
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin) return Forbid();

            var result = await _apiService.DeleteApparelAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Apparel deleted successfully!";
                return RedirectToPage(new { showList = true });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetReportDataAsync(string status, DateTime? startDate, DateTime? endDate)
        {
            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            if (int.TryParse(activeCampusClaim, out int acid)) filterCampusId = acid;

            var items = await _apiService.QueryApparelItemsAsync(status, startDate, endDate, filterCampusId);
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnGetApparelItemsAsync(int apparelId)
        {
            if (apparelId == 0) return new JsonResult(new List<ApparelItemDto>());
            
            var items = await _apiService.GetApparelItemsAsync(apparelId);
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnPostUpdateItemStatusAsync(int itemId, string status)
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager")) return new JsonResult(new { success = false, message = "Insufficient permissions." });

            var result = await _apiService.UpdateApparelItemStatusAsync(itemId, status);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostAddStockAsync(int id, int quantity)
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager") && !User.IsInRole("User")) return new JsonResult(new { success = false, message = "Insufficient permissions." });

            var result = await _apiService.AddApparelStockAsync(id, quantity);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnGetLogsAsync(int page = 1, int pageSize = 10)
        {
            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            if (int.TryParse(activeCampusClaim, out int acid)) filterCampusId = acid;

            var result = await _apiService.GetAuditLogsPaginatedAsync(filterCampusId, page, pageSize, "Apparel");
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetStatsAsync(int campusId)
        {
            var response = await _apiService.GetDashboardStatsAsync(campusId);
            return new JsonResult(response.Stats);
        }
    }
}
