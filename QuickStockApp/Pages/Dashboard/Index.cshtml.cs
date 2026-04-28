using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using QuickStockApp.Services;
using QuickStockApp.Models;

namespace QuickStockApp.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public int TotalAssets { get; set; }
        public int RoomCount { get; set; }
        public int DisabledRooms { get; set; }
        public string CampusName { get; set; } = "Selected Campus";
        public List<RecentActivityDto> Activities { get; set; } = new();

        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> OnGetAsync()
        {
            var activeName = User.FindFirst("ActiveCampusName")?.Value;
            var activeIdString = User.FindFirst("ActiveCampusId")?.Value;

            if (User.IsInRole("Admin"))
            {
                if (string.IsNullOrEmpty(activeIdString))
                {
                    return RedirectToPage("/Campuses");
                }
            }

            if (int.TryParse(activeIdString, out int campusId))
            {
                var response = await _apiService.GetDashboardStatsAsync(campusId);
                if (response.Stats != null)
                {
                    TotalAssets = response.Stats.TotalAssets;
                    RoomCount = response.Stats.RoomCount;
                    DisabledRooms = response.Stats.DisabledRooms;
                    Activities = response.Stats.RecentActivities;
                }
            }
            
            if (string.IsNullOrEmpty(activeName) && !string.IsNullOrEmpty(activeIdString))
            {
                if (int.TryParse(activeIdString, out int id))
                {
                    var campuses = await _apiService.GetCampusesAsync();
                    var campus = campuses.FirstOrDefault(c => c.CampusId == id);
                    if (campus != null)
                    {
                        activeName = campus.Name;
                    }
                }
            }

            CampusName = activeName ?? "Selected Campus";

            return Page();
        }

        public async Task<JsonResult> OnGetLogsAsync(int page = 1, int pageSize = 10)
        {
            var activeIdString = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeIdString, out int acid) ? acid : null;

            var result = await _apiService.GetAuditLogsPaginatedAsync(campusId, page, pageSize);
            return new JsonResult(result);
        }
    }

    public class RecentActivity
    {
        public string Action { get; set; } = "";
        public string Item { get; set; } = "";
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
