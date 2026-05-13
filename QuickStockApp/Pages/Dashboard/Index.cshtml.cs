using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // IT Assets
        public int TotalAssets { get; set; }
        public int RoomCount { get; set; }
        public int DisabledRooms { get; set; }
        public List<StatusCountDto> AssetsByStatus { get; set; } = new();
        public List<TypeCountDto> AssetsByType { get; set; } = new();
        public List<RecentActivityDto> RecentItActivities { get; set; } = new();

        // Apparel
        public int TotalApparelTypes { get; set; }
        public int TotalApparelInStock { get; set; }
        public int TotalApparelSold { get; set; }
        public List<CategoryCountDto> ApparelByCategory { get; set; } = new();
        public List<RecentActivityDto> RecentApparelActivities { get; set; } = new();

        // General
        public string CampusName { get; set; } = "Selected Campus";
        public List<RecentActivityDto> Activities { get; set; } = new();
        public bool CanSeeLogs { get; set; }

        // Home Economics / Furniture
        public int TotalFurniture { get; set; }
        public List<LocationCountDto> FurnitureByLocation { get; set; } = new();
        public List<ConditionCountDto> FurnitureByCondition { get; set; } = new();
        public List<RecentActivityDto> RecentFurnitureActivities { get; set; } = new();

        // Consumables
        public int TotalConsumableTypes { get; set; }
        public int TotalConsumableBalance { get; set; }
        public List<RecentActivityDto> RecentConsumableActivities { get; set; } = new();

        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> OnGetAsync()
        {
            CanSeeLogs = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin") || User.IsInRole("Manager") || User.IsInRole("User");

            var activeName = User.FindFirst("ActiveCampusName")?.Value;
            var activeIdString = User.FindFirst("ActiveCampusId")?.Value;

            if (string.IsNullOrEmpty(activeIdString))
                return RedirectToPage("/Campus/Campuses");

            if (int.TryParse(activeIdString, out int campusId))
            {
                var response = await _apiService.GetDashboardStatsAsync(campusId);
                if (response.Stats != null)
                {
                    var stats = response.Stats;
                    TotalAssets = stats.TotalAssets;
                    RoomCount = stats.RoomCount;
                    DisabledRooms = stats.DisabledRooms;
                    AssetsByStatus = stats.AssetsByStatus;
                    AssetsByType = stats.AssetsByType;
                    RecentItActivities = stats.RecentItActivities;

                    TotalApparelTypes = stats.TotalApparelTypes;
                    TotalApparelInStock = stats.TotalApparelInStock;
                    TotalApparelSold = stats.TotalApparelSold;
                    ApparelByCategory = stats.ApparelByCategory;
                    RecentApparelActivities = stats.RecentApparelActivities;

                    TotalFurniture = stats.TotalFurniture;
                    FurnitureByLocation = stats.FurnitureByLocation;
                    FurnitureByCondition = stats.FurnitureByCondition;
                    RecentFurnitureActivities = stats.RecentFurnitureActivities;

                    TotalConsumableTypes = stats.TotalConsumableTypes;
                    TotalConsumableBalance = stats.TotalConsumableBalance;
                    RecentConsumableActivities = stats.RecentConsumableActivities;

                    Activities = stats.RecentActivities;
                }
            }

            if (string.IsNullOrEmpty(activeName) && !string.IsNullOrEmpty(activeIdString))
            {
                if (int.TryParse(activeIdString, out int id))
                {
                    var campuses = await _apiService.GetCampusesAsync();
                    var campus = campuses.FirstOrDefault(c => c.CampusId == id);
                    if (campus != null) activeName = campus.Name;
                }
            }

            CampusName = activeName ?? "Selected Campus";
            return Page();
        }

        public async Task<JsonResult> OnGetLogsAsync(int page = 1, int pageSize = 10, string? entityType = null)
        {
            var activeIdString = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeIdString, out int acid) ? acid : null;

            var result = await _apiService.GetAuditLogsPaginatedAsync(campusId, page, pageSize, entityType);
            return new JsonResult(result);
        }
    }
}
