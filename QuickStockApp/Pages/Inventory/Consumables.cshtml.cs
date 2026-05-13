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

        public ConsumablesModel(IConsumableService consumableService)
        {
            _consumableService = consumableService;
        }

        public List<ConsumableDto> Consumables { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [BindProperty]
        public ConsumableDto NewConsumable { get; set; } = new();

        public async Task OnGetAsync()
        {
            var activeCampusId = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeCampusId, out int acid) ? acid : null;

            var response = await _consumableService.GetConsumablesAsync(campusId, SearchTerm, CurrentPage, PageSize);
            Consumables = response.Consumables;
            TotalItems = response.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();

            var (success, message) = await _consumableService.CreateConsumableAsync(NewConsumable);
            if (success)
            {
                TempData["SuccessMessage"] = "Consumable registered successfully.";
                return RedirectToPage();
            }

            ModelState.AddModelError("", message);
            await OnGetAsync();
            return Page();
        }

        public async Task<JsonResult> OnGetItemsAsync(int consumableId)
        {
            var items = await _consumableService.GetConsumableItemsAsync(consumableId);
            return new JsonResult(items);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int itemId, string status)
        {
            var success = await _consumableService.UpdateItemStatusAsync(itemId, status);
            return success ? new OkResult() : new BadRequestResult();
        }
    }
}
