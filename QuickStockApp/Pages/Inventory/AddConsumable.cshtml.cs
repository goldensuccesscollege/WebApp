using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Inventory
{
    [Authorize(Roles = "Admin,Manager,User,Library Admin,Home Economics Admin")]
    public class AddConsumableModel : PageModel
    {
        private readonly IConsumableService _consumableService;

        public AddConsumableModel(IConsumableService consumableService)
        {
            _consumableService = consumableService;
        }

        [BindProperty]
        public ConsumableDto NewConsumable { get; set; } = new();

        public void OnGet()
        {
            var activeCampusId = User.FindFirst("ActiveCampusId")?.Value;
            if (int.TryParse(activeCampusId, out int acid))
            {
                NewConsumable.CampusId = acid;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewConsumable.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    NewConsumable.CampusId = acid;
                }
            }

            if (NewConsumable.CampusId <= 0)
            {
                TempData["ErrorMessage"] = "Failed to identify campus. Please re-login.";
                return Page();
            }

            var (success, message) = await _consumableService.CreateConsumableAsync(NewConsumable);
            if (success)
            {
                TempData["SuccessMessage"] = "Consumable registered successfully.";
                return RedirectToPage("./Consumables", new { showList = true });
            }

            TempData["ErrorMessage"] = message;
            return Page();
        }
    }
}
