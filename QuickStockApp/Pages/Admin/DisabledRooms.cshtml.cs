using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DisabledRoomsModel : PageModel
    {
        private readonly IApiService _api;

        public DisabledRoomsModel(IApiService api)
        {
            _api = api;
        }

        public List<RoomDto> DisabledRooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            var activeCampusId = User.FindFirst("ActiveCampusId")?.Value;
            int? campusId = int.TryParse(activeCampusId, out int id) ? id : null;
            
            var allRooms = await _api.GetRoomsAsync(campusId);
            DisabledRooms = allRooms.Where(r => r.IsDisabled).ToList();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int roomId)
        {
            await _api.ToggleRoomStatusAsync(roomId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _api.DeleteRoomAsync(id);
            return RedirectToPage();
        }
    }
}
