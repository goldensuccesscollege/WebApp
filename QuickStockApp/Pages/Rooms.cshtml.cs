using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class RoomsModel : PageModel
    {
        private readonly IApiService _apiService;

        public RoomsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<RoomDto> Rooms { get; set; } = new();
        public List<CampusDto> AssignedCampuses { get; set; } = new();

        [BindProperty]
        public RoomDto Room { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? campusId = null)
        {
            int? filterCampusId = null;
            var activeCampusClaim = User.FindFirst("ActiveCampusId")?.Value;

            if (User.IsInRole("Admin"))
            {
                if (int.TryParse(activeCampusClaim, out int acid))
                {
                    filterCampusId = acid;
                }
                else
                {
                    return RedirectToPage("/Campuses");
                }
            }
            else
            {
                var userCampusIdStr = User.FindFirst("CampusId")?.Value;
                if (int.TryParse(userCampusIdStr, out int userCid))
                {
                    filterCampusId = userCid;
                }
            }

            Rooms = await _apiService.GetRoomsAsync(filterCampusId);
            
            var allCampuses = await _apiService.GetCampusesAsync();
            if (User.IsInRole("Admin"))
            {
                AssignedCampuses = allCampuses;
            }
            else
            {
                var userCampusIds = User.FindAll("CampusId").Select(c => int.Parse(c.Value)).ToList();
                AssignedCampuses = allCampuses.Where(c => userCampusIds.Contains(c.CampusId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (Room.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    Room.CampusId = acid;
                }
                else
                {
                    var userCampusIds = User.FindAll("CampusId").Select(c => int.Parse(c.Value)).ToList();
                    if (userCampusIds.Count == 1)
                    {
                        Room.CampusId = userCampusIds[0];
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                Rooms = await _apiService.GetRoomsAsync();
                return Page();
            }

            var (success, message) = await _apiService.AddRoomAsync(Room);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Room added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid || Room.RoomId <= 0)
            {
                Rooms = await _apiService.GetRoomsAsync();
                return Page();
            }

            var (success, message) = await _apiService.UpdateRoomAsync(Room);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Room updated successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (success, message) = await _apiService.DeleteRoomAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Room deleted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int roomId)
        {
            var (success, message) = await _apiService.ToggleRoomStatusAsync(roomId);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Room status updated!";
            return RedirectToPage();
        }
    }
}
