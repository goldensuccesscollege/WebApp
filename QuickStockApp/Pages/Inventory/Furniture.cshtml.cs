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
    public class FurnitureModel : PageModel
    {
        private readonly IApiService _apiService;

        public FurnitureModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<FurnitureDto> Furnitures { get; set; } = new();
        public List<RoomDto> Rooms { get; set; } = new();

        [BindProperty]
        public FurnitureDto Furniture { get; set; } = new();

        public string? SelectedCampusName { get; set; }

        public async Task<IActionResult> OnGetAsync(string? searchTerm = null)
        {
            var hasAccess = User.IsInRole("Admin") || 
                            User.IsInRole("Home Economics Admin") ||
                            User.FindFirst("CanAccessHomeEconomics")?.Value == "True";

            if (!hasAccess)
            {
                return Forbid();
            }

            var campusIdStr = User.FindFirst("ActiveCampusId")?.Value;
            SelectedCampusName = User.FindFirst("ActiveCampusName")?.Value;

            if (int.TryParse(campusIdStr, out int campusId))
            {
                Furnitures = await _apiService.GetFurnituresAsync(campusId: campusId, searchTerm: searchTerm);
                Rooms = await _apiService.GetRoomsAsync(campusId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddFurnitureAsync()
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager") && !User.IsInRole("Staff")) return Forbid();

            if (Furniture.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    Furniture.CampusId = acid;
                }
            }

            var result = await _apiService.AddFurnitureAsync(Furniture);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Furniture added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateFurnitureAsync()
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager")) return Forbid();

            var result = await _apiService.UpdateFurnitureAsync(Furniture);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Furniture updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferFurnitureAsync(int furnitureId, int targetRoomId)
        {
            if (User.IsInRole("Viewer")) return Forbid();

            var result = await _apiService.TransferFurnitureAsync(furnitureId, targetRoomId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Furniture transferred successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetStatsAsync(int campusId)
        {
            var response = await _apiService.GetDashboardStatsAsync(campusId);
            return new JsonResult(response.Stats);
        }
    }
}

