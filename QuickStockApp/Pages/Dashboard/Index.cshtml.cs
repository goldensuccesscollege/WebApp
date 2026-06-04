using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuickStockApp.Services;
using QuickStockApp.Models;

namespace QuickStockApp.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IConsumableService _consumableService;

        public IndexModel(IApiService apiService, IConsumableService consumableService)
        {
            _apiService = apiService;
            _consumableService = consumableService;
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

        // Consumables
        public int TotalConsumableTypes { get; set; }
        public int TotalConsumableBalance { get; set; }
        public List<RecentActivityDto> RecentConsumableActivities { get; set; } = new();
        public List<ConsumableResponse> Consumables { get; set; } = new();

        // Home Economics / Furniture
        public int TotalFurniture { get; set; }
        public List<LocationCountDto> FurnitureByLocation { get; set; } = new();
        public List<ConditionCountDto> FurnitureByCondition { get; set; } = new();
        public List<RecentActivityDto> RecentFurnitureActivities { get; set; } = new();

        // Enhanced Dashboard Properties
        public int ActiveAssets { get; set; }
        public int CriticalAssets { get; set; }
        public int AssetHealthPercentage { get; set; }
        public string? MostCommonAssetType { get; set; }
        public string? MostProblematicRoom { get; set; }
        public int MonthlyActivities { get; set; }
        public int NewAssetsThisMonth { get; set; }
        public string? SystemOverview { get; set; }
        public List<string> Alerts { get; set; } = new();
        public List<RoomAssetDto> AssetsByRoom { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> OnGetAsync()
        {
            CanSeeLogs = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Employee");

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
                    RecentConsumableActivities = stats.RecentConsumableActivities ?? new();
                    Consumables = await _consumableService.GetAllConsumablesAsync(campusId);

                    Activities = stats.RecentActivities;

                    ActiveAssets = stats.ActiveAssets;
                    CriticalAssets = stats.CriticalAssets;
                    AssetHealthPercentage = stats.AssetHealthPercentage;
                    MostCommonAssetType = stats.MostCommonAssetType;
                    MostProblematicRoom = stats.MostProblematicRoom;
                    MonthlyActivities = stats.MonthlyActivities;
                    NewAssetsThisMonth = stats.NewAssetsThisMonth;
                    SystemOverview = stats.SystemOverview;
                    Alerts = stats.Alerts ?? new();
                    AssetsByRoom = stats.AssetsByRoom ?? new();
                    RecentActivities = stats.RecentActivities ?? new();
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

        public async Task<JsonResult> OnGetNotificationsAsync()
        {
            var activeIdString = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeIdString, out int acid) ? acid : null;
            
            var requests = await _consumableService.GetConsumableRequestsAsync(campusId);
            
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isEmployee = User.IsInRole("Employee");
            var isManager = User.IsInRole("Manager");
            var isAdmin = User.IsInRole("Admin");

            var notifications = new List<object>();

            if (isAdmin || isManager)
            {
                // Admin and Managers should see pending requests that need approval
                var pending = requests.Where(r => r.Status == "Pending").OrderByDescending(r => r.Timestamp).Take(10);
                foreach (var req in pending)
                {
                    notifications.Add(new
                    {
                        id = req.Id,
                        title = $"New Request: {req.RequestType} {req.ProductName}",
                        message = $"Employee {req.RequestorName} requested to {req.RequestType.ToLower()} {req.Count} {req.ProductType}.",
                        timestamp = req.Timestamp,
                        status = req.Status,
                        url = "/Inventory/ConsumableRequests"
                    });
                }
            }
            
            if (isEmployee)
            {
                // Employee should see approved or rejected requests as notifications
                var myResolved = requests
                    .Where(r => r.RequestorId == currentUserId && r.Status != "Pending")
                    .OrderByDescending(r => r.Timestamp)
                    .Take(10);
                
                foreach (var req in myResolved)
                {
                    var isApproved = req.Status == "Approved";
                    var message = isApproved 
                        ? $"Your request to {req.RequestType.ToLower()} {req.Count} {req.ProductType} of {req.ProductName} was approved!"
                        : $"Your request to {req.RequestType.ToLower()} {req.Count} {req.ProductType} of {req.ProductName} was rejected. Reason: {req.RejectionReason}";
                    
                    notifications.Add(new
                    {
                        id = req.Id,
                        title = isApproved ? "Request Approved" : "Request Rejected",
                        message = message,
                        timestamp = req.Timestamp,
                        status = req.Status,
                        url = "/Inventory/ConsumableRequests"
                    });
                }
            }

            return new JsonResult(notifications);
        }
    }
}
