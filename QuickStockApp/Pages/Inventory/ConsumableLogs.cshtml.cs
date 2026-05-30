using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Inventory
{
    [Authorize]
    public class ConsumableLogsModel : PageModel
    {
        private readonly IReportService _reportService;

        public ConsumableLogsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public List<AuditLogDto> ConsumableLogs { get; set; } = new();
        public string CampusName { get; set; } = "Selected Campus";

        [BindProperty(SupportsGet = true)]
        public int? SelectedCampusId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var hasAccess = User.IsInRole("Admin") || 
                            User.FindFirst("CanAccessConsumables")?.Value == "True";

            if (!hasAccess)
            {
                return Forbid();
            }

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            CampusName = User.FindFirst("ActiveCampusName")?.Value ?? "Selected Campus";

            int? activeCampusId = null;
            if (int.TryParse(campusIdStr, out int acid))
            {
                activeCampusId = acid;
            }

            var logsResult = await _reportService.GetAuditLogsPaginatedAsync(activeCampusId, 1, 500, "Consumable");
            ConsumableLogs = logsResult?.Logs ?? new();

            return Page();
        }

        public string GetLogCount(string details)
        {
            if (string.IsNullOrEmpty(details)) return "0";
            var parts = details.Split('|');
            foreach (var part in parts)
            {
                if (part.Contains("Count:"))
                {
                    return part.Replace("Count:", "").Trim();
                }
            }
            return "0";
        }

        public string GetLogType(string details, string fallbackType = "N/A")
        {
            if (string.IsNullOrEmpty(details)) return fallbackType;
            var parts = details.Split('|');
            foreach (var part in parts)
            {
                if (part.Contains("Type:"))
                {
                    return part.Replace("Type:", "").Trim();
                }
            }
            return fallbackType;
        }
    }
}
