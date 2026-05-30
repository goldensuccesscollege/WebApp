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

        // Expose data layers directly to the .cshtml layout view
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

        [TempData]
        public string? FeedbackMessage { get; set; }

        [TempData]
        public bool IsSuccessState { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var hasAccess = User.IsInRole("Admin") || 
                            User.FindFirst("CanAccessConsumables")?.Value == "True";

            if (!hasAccess)
            {
                return Forbid();
            }

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            int? activeCampusId = SelectedCampusId > 0 ? SelectedCampusId : null;
            if (!activeCampusId.HasValue && int.TryParse(campusIdStr, out int acid))
            {
                activeCampusId = acid;
            }

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
                        var parts = log.Details.Split('|');
                        foreach (var part in parts)
                        {
                            if (part.Contains("Count:"))
                            {
                                int.TryParse(part.Replace("Count:", "").Trim(), out count);
                            }
                        }
                    }

                    if (log.Action == "Add Stock" || log.Action == "Create")
                    {
                        ThisWeekInflow += count;
                    }
                    else if (log.Action == "Deduct Stock")
                    {
                        ThisWeekOutflow += count;
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddStockAsync()
        {
            if (QuantityDelta <= 0)
            {
                FeedbackMessage = "Quantity must be greater than zero.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            // Resolve SelectedCampusId from user profile claim if not loaded
            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            int campusId = 1;
            if (int.TryParse(campusIdStr, out int acid))
            {
                campusId = acid;
            }

            // Intercept if User is in Staff Role
            if (User.IsInRole("Staff"))
            {
                // Fetch details to map Name and Type to the Request
                var consumables = await _consumableService.GetAllConsumablesAsync(campusId);
                var item = consumables.Find(c => c.Id == TargetItemId);

                var requestCommand = new CreateConsumableRequestCommand
                {
                    RequestType = "Add",
                    ProductName = item?.ProductName ?? "Unknown Item",
                    ProductType = item?.ProductType ?? "pieces",
                    Count = QuantityDelta,
                    TargetItemId = TargetItemId,
                    CampusId = campusId
                };

                var (reqSuccess, reqMessage) = await _consumableService.CreateConsumableRequestAsync(requestCommand);
                FeedbackMessage = reqMessage;
                IsSuccessState = reqSuccess;
                return RedirectToPage(new { SelectedCampusId });
            }

            var command = new AddStockCommand { Id = TargetItemId, Quantity = QuantityDelta };
            var (success, message) = await _consumableService.AddStockAsync(command);

            FeedbackMessage = message;
            IsSuccessState = success;

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

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            int campusId = 1;
            if (int.TryParse(campusIdStr, out int acid))
            {
                campusId = acid;
            }

            // Intercept if User is in Staff Role
            if (User.IsInRole("Staff"))
            {
                var consumables = await _consumableService.GetAllConsumablesAsync(campusId);
                var item = consumables.Find(c => c.Id == TargetItemId);

                var requestCommand = new CreateConsumableRequestCommand
                {
                    RequestType = "Deduct",
                    ProductName = item?.ProductName ?? "Unknown Item",
                    ProductType = item?.ProductType ?? "pieces",
                    Count = QuantityDelta,
                    TargetItemId = TargetItemId,
                    CampusId = campusId
                };

                var (reqSuccess, reqMessage) = await _consumableService.CreateConsumableRequestAsync(requestCommand);
                FeedbackMessage = reqMessage;
                IsSuccessState = reqSuccess;
                return RedirectToPage(new { SelectedCampusId });
            }

            var command = new DeductStockCommand { Id = TargetItemId, Quantity = QuantityDelta };
            var (success, message) = await _consumableService.DeductStockAsync(command);

            FeedbackMessage = message;
            IsSuccessState = success;

            return RedirectToPage(new { SelectedCampusId });
        }

        public async Task<IActionResult> OnPostCreateConsumableAsync()
        {
            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            int campusId = 1;
            if (int.TryParse(campusIdStr, out int acid))
            {
                campusId = acid;
            }
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

            // Intercept if User is in Staff Role
            if (User.IsInRole("Staff"))
            {
                var requestCommand = new CreateConsumableRequestCommand
                {
                    RequestType = "Create",
                    ProductName = NewConsumable.ProductName,
                    ProductType = NewConsumable.ProductType,
                    Count = NewConsumable.Count,
                    CampusId = campusId
                };

                var (reqSuccess, reqMessage) = await _consumableService.CreateConsumableRequestAsync(requestCommand);
                FeedbackMessage = reqMessage;
                IsSuccessState = reqSuccess;
                return RedirectToPage(new { SelectedCampusId });
            }

            var (success, message) = await _consumableService.CreateConsumableAsync(NewConsumable);
            FeedbackMessage = message;
            IsSuccessState = success;

            return RedirectToPage(new { SelectedCampusId });
        }
    }
}