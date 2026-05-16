using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Inventory
{
    [Authorize]
    public class ConsumablesModel : PageModel
    {
        private readonly IConsumableService _consumableService;
        private readonly IApiService _apiService;

        public ConsumablesModel(IConsumableService consumableService, IApiService apiService)
        {
            _consumableService = consumableService;
            _apiService = apiService;
        }

        public List<ConsumableDto> Consumables { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public bool ShowOutOnly { get; set; } = false;


        public async Task OnGetAsync()
        {
            var activeCampusId = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeCampusId, out int acid) ? acid : null;

            var response = await _consumableService.GetConsumablesAsync(campusId, SearchTerm, CurrentPage, PageSize, ShowOutOnly);
            Consumables = response.Consumables;
            TotalItems = response.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        }


        public async Task<JsonResult> OnGetItemsAsync(int consumableId, bool showOutOnly = false)
        {
            var items = await _consumableService.GetConsumableItemsAsync(consumableId, showOutOnly);
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int itemId, string status)
        {
            var success = await _consumableService.UpdateItemStatusAsync(itemId, status);
            return success ? new OkResult() : new BadRequestResult();
        }

        public async Task<IActionResult> OnPostUpdateAsync(ConsumableDto consumable)
        {
            var success = await _consumableService.UpdateConsumableAsync(consumable);
            return success ? new JsonResult(new { success = true }) : new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostRestockAsync(int consumableId, int quantity)
        {
            var success = await _consumableService.RestockConsumableAsync(consumableId, quantity);
            return success ? new JsonResult(new { success = true }) : new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnGetLogsAsync(int page = 1, int pageSize = 10)
        {
            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            if (int.TryParse(activeCampusClaim, out int acid)) filterCampusId = acid;

            var result = await _apiService.GetAuditLogsPaginatedAsync(filterCampusId, page, pageSize, "Consumable");
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetStatsAsync(int campusId)
        {
            var response = await _apiService.GetDashboardStatsAsync(campusId);
            return new JsonResult(response.Stats);
        }

        // ---- Out Request Workflow ----

        public async Task<IActionResult> OnPostRequestOutAsync(int itemId)
        {
            var (success, message) = await _consumableService.RequestItemOutAsync(itemId);
            return new JsonResult(new { success, message });
        }

        public async Task<JsonResult> OnGetPendingRequestsAsync()
        {
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            int campusId = int.TryParse(activeCampusClaim, out int acid) ? acid : 0;
            var requests = await _consumableService.GetPendingOutRequestsAsync(campusId);
            return new JsonResult(requests);
        }

        public async Task<JsonResult> OnGetRequestHistoryAsync(string? status = null)
        {
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;
            int campusId = int.TryParse(activeCampusClaim, out int acid) ? acid : 0;
            
            string? userId = null;
            if (!(User.IsInRole("Admin") || User.IsInRole("Manager")))
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var requests = await _consumableService.GetOutRequestHistoryAsync(campusId, status, userId);
            return new JsonResult(requests);
        }

        public async Task<IActionResult> OnPostApproveRequestAsync(int requestId)
        {
            var (success, message) = await _consumableService.ApproveOutRequestAsync(requestId);
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int requestId, string reason)
        {
            var (success, message) = await _consumableService.RejectOutRequestAsync(requestId, reason ?? "");
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostCancelRequestAsync(int requestId)
        {
            var (success, message) = await _consumableService.CancelOutRequestAsync(requestId);
            return new JsonResult(new { success, message });
        }
    }
}
