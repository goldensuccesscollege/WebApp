using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Inventory
{
    [Authorize]
    public class ConsumableLedgerModel : PageModel
    {
        private readonly IConsumableService _consumableService;

        public ConsumableLedgerModel(IConsumableService consumableService)
        {
            _consumableService = consumableService;
        }

        public List<ConsumableLedgerEntryDto> LedgerEntries { get; set; } = new();
        public List<string> ProductNames { get; set; } = new();
        public string CampusName { get; set; } = "Selected Campus";

        // Summary KPIs
        public int TotalIn { get; set; }
        public int TotalOut { get; set; }
        public int UniqueProductCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedCampusId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.IsInRole("Employee"))
            {
                return Forbid();
            }

            var hasAccess = User.IsInRole("Admin") ||
                            User.FindFirst("CanAccessConsumables")?.Value == "True";

            if (!hasAccess)
            {
                return Forbid();
            }

            CampusName = User.FindFirst("ActiveCampusName")?.Value ?? "Selected Campus";

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            int? activeCampusId = SelectedCampusId > 0 ? SelectedCampusId : null;
            if (!activeCampusId.HasValue && int.TryParse(campusIdStr, out int acid))
            {
                activeCampusId = acid;
            }

            LedgerEntries = await _consumableService.GetConsumableLedgerAsync(activeCampusId);

            // Build distinct sorted product name list for the filter dropdown
            ProductNames = LedgerEntries
                .Select(e => e.ProductName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            // Calculate KPI totals
            TotalIn = LedgerEntries.Sum(e => e.In);
            TotalOut = LedgerEntries.Sum(e => e.Out);
            UniqueProductCount = LedgerEntries
                .Select(e => e.ProductId)
                .Distinct()
                .Count();

            return Page();
        }
    }
}