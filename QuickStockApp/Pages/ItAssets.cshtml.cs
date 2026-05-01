using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ItAssetsModel : PageModel
    {
        private readonly IApiService _apiService;

        public ItAssetsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<Models.ItAssetDto> Assets { get; set; } = new();
        public List<Models.RoomDto> Rooms { get; set; } = new();
        public List<Models.CampusDto> AssignedCampuses { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? SelectedRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public Models.ItAssetDto Asset { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? campusId = null)
        {
            var canAccess = User.IsInRole("Admin") || (User.FindFirst("CanAccessITAssets")?.Value == "True");
            if (!canAccess)
            {
                return Forbid();
            }

            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;

            if (int.TryParse(activeCampusClaim, out int acid))
            {
                filterCampusId = acid;
            }
            else
            {
                return RedirectToPage("/Campuses");
            }

            Rooms = await _apiService.GetRoomsAsync(filterCampusId);
            Assets = await _apiService.GetItAssetsAsync(SelectedRoomId, filterCampusId, SearchTerm);

            var allCampuses = await _apiService.GetCampusesAsync();
            if (User.IsInRole("Admin"))
            {
                AssignedCampuses = allCampuses;
            }
            else
            {
                var userIds = User.FindAll("CampusId").Select(c => int.Parse(c.Value)).ToList();
                AssignedCampuses = allCampuses.Where(c => userIds.Contains(c.CampusId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (Asset.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    Asset.CampusId = acid;
                }
                else
                {
                    var userIds = User.FindAll("CampusId").Select(c => int.Parse(c.Value)).ToList();
                    if (userIds.Count == 1)
                    {
                        Asset.CampusId = userIds[0];
                    }
                }
            }

            var result = await _apiService.AddItAssetAsync(Asset);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Asset added successfully!";
                return RedirectToPage();
            }
            
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (Asset.Id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid Asset ID for update.");
            }
            else
            {
                var result = await _apiService.UpdateItAssetAsync(Asset);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Asset updated successfully!";
                    return RedirectToPage();
                }
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _apiService.DeleteItAssetAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Asset deleted successfully!";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferAsync(int assetId, int targetRoomId)
        {
            var result = await _apiService.TransferItAssetAsync(assetId, targetRoomId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Asset transferred successfully!";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }
    }
}
