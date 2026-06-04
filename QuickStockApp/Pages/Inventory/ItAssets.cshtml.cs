using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;
using System.Linq;

namespace QuickStockApp.Pages.Inventory
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
        public List<Models.ItAssetDto> AllAssets { get; set; } = new();
        public List<Models.RoomDto> Rooms { get; set; } = new();
        public List<Models.CampusDto> AssignedCampuses { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? SelectedRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        [BindProperty]
        public Models.ItAssetDto Asset { get; set; } = new();




        public async Task<IActionResult> OnGetAsync(int? campusId = null)
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            var canAccess = isAnyAdmin || (User.FindFirst("CanAccessITAssets")?.Value == "True");
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
                return RedirectToPage("/Campus/Campuses");
            }

            Rooms = await _apiService.GetRoomsAsync(filterCampusId);
            var allAssets = await _apiService.GetItAssetsAsync(SelectedRoomId, filterCampusId, SearchTerm);
            
            // Sort by newest first
            var sortedAssets = allAssets.OrderByDescending(a => a.DateAdded).ToList();
            
            // Calculate pagination
            TotalPages = (int)Math.Ceiling(sortedAssets.Count / (double)PageSize);
            if (CurrentPage < 1) CurrentPage = 1;
            if (TotalPages > 0 && CurrentPage > TotalPages) CurrentPage = TotalPages;

            AllAssets = sortedAssets;

            Assets = sortedAssets
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

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
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager") && !User.IsInRole("Employee")) return Forbid();

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
                return RedirectToPage(new { SelectedRoomId = Asset.RoomId, tab = "list" });
            }
            
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }




        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager")) return Forbid();

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
                    return RedirectToPage(new { SelectedRoomId = Asset.RoomId, tab = "list" });
                }
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin") || User.IsInRole("Home Economics Admin");
            if (!isAnyAdmin) return Forbid();

            var result = await _apiService.DeleteItAssetAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Asset deleted successfully!";
                return RedirectToPage(new { SelectedRoomId = SelectedRoomId, tab = "list" });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferAsync(int assetId, int targetRoomId)
        {
            if (User.IsInRole("Viewer")) return Forbid(); // Admin, Manager, and User can transfer

            var result = await _apiService.TransferItAssetAsync(assetId, targetRoomId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Asset transferred successfully!";
                return RedirectToPage(new { SelectedRoomId = targetRoomId, tab = "list" });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetStatsAsync(int campusId)
        {
            var response = await _apiService.GetDashboardStatsAsync(campusId);
            return new JsonResult(response.Stats);
        }
    }
}
