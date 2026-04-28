using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementModel : PageModel
    {
        private readonly IApiService _api;

        public UserManagementModel(IApiService api)
        {
            _api = api;
        }

        public List<UserManagementDto> Users { get; set; } = new();
        public List<CampusDto> AllCampuses { get; set; } = new();

        [BindProperty]
        public CreateUserDto NewUser { get; set; } = new();

        [BindProperty]
        public UpdateUserDto EditUser { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _api.GetUsersForManagementAsync();
            AllCampuses = await _api.GetCampusesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var result = await _api.CreateUserAsync(NewUser);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await OnGetAsync();
                return Page();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var result = await _api.UpdateUserAsync(id, EditUser);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await OnGetAsync();
                return Page();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _api.DeleteUserAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            await _api.ToggleUserStatusAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostGrantAccessAsync(int userId, int campusId)
        {
            await _api.AddUserCampusAccessAsync(userId, campusId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(int userId, int campusId)
        {
            await _api.ToggleUserCampusBlockAsync(userId, campusId);
            return RedirectToPage();
        }
    }
}
