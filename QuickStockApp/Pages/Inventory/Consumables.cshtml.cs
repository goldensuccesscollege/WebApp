using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickStockApp.Pages
{
    [Authorize]
    public class ConsumablesModel : PageModel
    {
        private readonly IConsumableService _consumableService;
        private readonly IReportService _reportService;

        public ConsumablesModel(IConsumableService consumableService, IReportService reportService)
        {
            _consumableService = consumableService;
            _reportService = reportService;
        }

        public List<ConsumableResponse> ConsumablesList { get; set; } = new();
        public int ThisWeekInflow { get; set; }
        public int ThisWeekOutflow { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedCampusId { get; set; }

        [BindProperty]
        public int TargetItemId { get; set; }

        [BindProperty]
        public CreateConsumableCommand NewConsumable { get; set; } = new();

        [BindProperty]
        public int QuantityDelta { get; set; }

        [BindProperty]
        public System.DateTime RequestDate { get; set; } = System.DateTime.Today;

        [TempData]
        public string? FeedbackMessage { get; set; }

        [TempData]
        public bool IsSuccessState { get; set; }

        // -----------------------------------------------------------------------
        // Resolves the active campus from the user's JWT claims.
        // -----------------------------------------------------------------------
        private int ResolveCampusId()
        {
            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            return int.TryParse(campusIdStr, out int id) ? id : 1;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var hasAccess = User.IsInRole("Admin") ||
                            User.FindFirst("CanAccessConsumables")?.Value == "True";

            if (!hasAccess)
                return Forbid();

            int? activeCampusId = SelectedCampusId > 0 ? SelectedCampusId : null;
            if (!activeCampusId.HasValue)
                activeCampusId = ResolveCampusId();

            ConsumablesList = await _consumableService.GetAllConsumablesAsync(activeCampusId);

            var logsResult = await _reportService.GetAuditLogsPaginatedAsync(activeCampusId, 1, 500, "Consumable");
            var logsList = logsResult?.Logs ?? new List<AuditLogDto>();

            ThisWeekInflow = 0;
            ThisWeekOutflow = 0;

            var cutoffDate = System.DateTime.UtcNow.AddDays(-7);
            foreach (var log in logsList)
            {
                if (log.Timestamp >= cutoffDate)
                {
                    int count = 0;
                    if (!string.IsNullOrEmpty(log.Details))
                    {
                        foreach (var part in log.Details.Split('|'))
                        {
                            if (part.Contains("Count:"))
                                int.TryParse(part.Replace("Count:", "").Trim(), out count);
                        }
                    }

                    if (log.Action == "Add Stock" || log.Action == "Create")
                        ThisWeekInflow += count;
                    else if (log.Action == "Deduct Stock")
                        ThisWeekOutflow += count;
                }
            }

            return Page();
        }

        // -----------------------------------------------------------------------
        // ALL roles (Admin, Manager, Employee) submit through the Request pipeline.
        // Admin / Manager can approve their own requests on the Requests page.
        // Employee requests stay Pending until an Admin/Manager approves them.
        // -----------------------------------------------------------------------

        public async Task<IActionResult> OnPostAddStockAsync()
        {
            if (User.IsInRole("Employee"))
            {
                FeedbackMessage = "Access denied: Employees cannot request stock additions.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            if (QuantityDelta <= 0)
            {
                FeedbackMessage = "Quantity must be greater than zero.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            var campusId = ResolveCampusId();
            var consumables = await _consumableService.GetAllConsumablesAsync(campusId);
            var item = consumables.Find(c => c.Id == TargetItemId);

            var (success, message) = await _consumableService.CreateConsumableRequestAsync(
                new CreateConsumableRequestCommand
                {
                    RequestType  = "Add",
                    ProductName  = item?.ProductName ?? "Unknown Item",
                    ProductType  = item?.ProductType ?? "pieces",
                    Count        = QuantityDelta,
                    TargetItemId = TargetItemId,
                    CampusId     = campusId,
                    RequestorId  = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    RequestorName = User.Identity?.Name
                });

            FeedbackMessage = message;
            IsSuccessState  = success;
            return RedirectToPage(new { SelectedCampusId });
        }

        public async Task<IActionResult> OnPostDeductStockAsync()
        {
            if (QuantityDelta <= 0)
            {
                FeedbackMessage = "Quantity must be greater than zero.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            var campusId = ResolveCampusId();
            var consumables = await _consumableService.GetAllConsumablesAsync(campusId);
            var item = consumables.Find(c => c.Id == TargetItemId);

            var (success, message) = await _consumableService.CreateConsumableRequestAsync(
                new CreateConsumableRequestCommand
                {
                    RequestType  = "Deduct",
                    ProductName  = item?.ProductName ?? "Unknown Item",
                    ProductType  = item?.ProductType ?? "pieces",
                    Count        = QuantityDelta,
                    TargetItemId = TargetItemId,
                    CampusId     = campusId,
                    RequestorId  = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    RequestorName = User.Identity?.Name,
                    Timestamp    = RequestDate
                });

            FeedbackMessage = message;
            IsSuccessState  = success;
            return RedirectToPage(new { SelectedCampusId });
        }

        public async Task<IActionResult> OnPostCreateConsumableAsync()
        {
            if (User.IsInRole("Employee"))
            {
                FeedbackMessage = "Access denied: Employees cannot request consumable creation.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            var campusId = ResolveCampusId();
            NewConsumable.CampusId = campusId;

            if (string.IsNullOrWhiteSpace(NewConsumable.ProductName))
            {
                FeedbackMessage = "Product Name is required.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            if (string.IsNullOrWhiteSpace(NewConsumable.ProductType))
            {
                FeedbackMessage = "Product Type / Unit is required.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            if (NewConsumable.Count < 0)
            {
                FeedbackMessage = "Count cannot be negative.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            var (success, message) = await _consumableService.CreateConsumableRequestAsync(
                new CreateConsumableRequestCommand
                {
                    RequestType = "Create",
                    ProductName = NewConsumable.ProductName,
                    ProductType = NewConsumable.ProductType,
                    Count       = NewConsumable.Count,
                    CampusId    = campusId,
                    RequestorId  = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    RequestorName = User.Identity?.Name
                });

            FeedbackMessage = message;
            IsSuccessState  = success;
            return RedirectToPage(new { SelectedCampusId });
        }
    }
}