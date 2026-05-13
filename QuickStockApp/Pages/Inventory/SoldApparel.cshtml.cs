using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;
using QuickStockApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuickStockApp.Pages.Inventory
{
    [Authorize]
    public class SoldApparelModel : PageModel
    {
        private readonly IApiService _apiService;

        public SoldApparelModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<ApparelItemDto> SoldItems { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

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

            if (int.TryParse(activeCampusClaim, out int acid))
            {
                filterCampusId = acid;
            }

            var result = await _apiService.GetSoldItemsAsync(filterCampusId, CurrentPage, 6);
            SoldItems = result.Items;
            TotalPages = result.TotalPages;
            TotalItems = result.TotalItems;

            return Page();
        }

        public async Task<IActionResult> OnGetReportDataAsync(string status, DateTime? startDate, DateTime? endDate)
        {
            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            if (int.TryParse(activeCampusClaim, out int acid)) filterCampusId = acid;

            var items = await _apiService.QueryApparelItemsAsync(status, startDate, endDate, filterCampusId);
            return new JsonResult(items);
        }
    }
}
