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
    public class ConsumableRequestsModel : PageModel
    {
        private readonly IConsumableService _consumableService;

        public ConsumableRequestsModel(IConsumableService consumableService)
        {
            _consumableService = consumableService;
        }

        public List<ConsumableRequestDto> ConsumableRequests { get; set; } = new();
        public string CampusName { get; set; } = "Selected Campus";

        [BindProperty(SupportsGet = true)]
        public int? SelectedCampusId { get; set; }

        [TempData]
        public string? FeedbackMessage { get; set; }

        [TempData]
        public bool IsSuccessState { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var hasAccess = User.IsInRole("Admin") ||
                            User.FindFirst("CanAccessConsumables")?.Value == "True";

            if (!hasAccess)
                return Forbid();

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            CampusName = User.FindFirst("ActiveCampusName")?.Value ?? "Selected Campus";

            int? activeCampusId = SelectedCampusId > 0 ? SelectedCampusId : null;
            if (!activeCampusId.HasValue && int.TryParse(campusIdStr, out int acid))
                activeCampusId = acid;

            ConsumableRequests = await _consumableService.GetConsumableRequestsAsync(activeCampusId);

            return Page();
        }

        // Only Admin and Manager can approve/reject requests.
        // Employee role can only view their own submitted requests.
        public async Task<IActionResult> OnPostApproveRequestAsync(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                return Forbid();

            var (success, message) = await _consumableService.ApproveConsumableRequestAsync(id);
            FeedbackMessage = message;
            IsSuccessState = success;
            return RedirectToPage(new { SelectedCampusId });
        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int id, string reason)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                return Forbid();

            if (string.IsNullOrWhiteSpace(reason))
            {
                FeedbackMessage = "A reason for rejection must be provided.";
                IsSuccessState = false;
                return RedirectToPage(new { SelectedCampusId });
            }

            var rejectCommand = new RejectConsumableRequestCommand { RejectionReason = reason };
            var (success, message) = await _consumableService.RejectConsumableRequestAsync(id, rejectCommand);
            FeedbackMessage = message;
            IsSuccessState = success;
            return RedirectToPage(new { SelectedCampusId });
        }
    }
}
