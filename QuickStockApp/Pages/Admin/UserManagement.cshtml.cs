using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuickStockApp.Pages.Admin
{
    [Authorize(Roles = "Admin,Library Admin,Home Economics Admin")]
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
            if (IsSelf(id))
            {
                TempData["ErrorMessage"] = "You cannot modify your own account from here.";
                return RedirectToPage();
            }

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
            if (IsSelf(id))
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToPage();
            }
            await _api.DeleteUserAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            if (IsSelf(id))
            {
                TempData["ErrorMessage"] = "You cannot disable your own account.";
                return RedirectToPage();
            }
            await _api.ToggleUserStatusAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostGrantAccessAsync(int userId, int campusId)
        {
            if (IsSelf(userId))
            {
                TempData["ErrorMessage"] = "You cannot modify your own campus access from here.";
                return RedirectToPage();
            }
            await _api.AddUserCampusAccessAsync(userId, campusId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(int userId, int campusId)
        {
            if (IsSelf(userId))
            {
                TempData["ErrorMessage"] = "You cannot block yourself from a campus.";
                return RedirectToPage();
            }
            await _api.ToggleUserCampusBlockAsync(userId, campusId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleITAccessAsync(int id)
        {
            if (IsSelf(id)) return RedirectToPage();
            await _api.ToggleUserITAccessAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAPAccessAsync(int id)
        {
            if (IsSelf(id)) return RedirectToPage();
            await _api.ToggleUserAPAccessAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleMessageAccessAsync(int id)
        {
            if (IsSelf(id)) return RedirectToPage();
            await _api.ToggleUserMessageAccessAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleLibraryAccessAsync(int id)
        {
            await _api.ToggleUserLibraryAccessAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleHomeEconomicsAccessAsync(int id)
        {
            await _api.ToggleUserHomeEconomicsAccessAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleConsumablesAccessAsync(int id)
        {
            await _api.ToggleUserConsumablesAccessAsync(id);
            return RedirectToPage();
        }

        private bool IsSelf(int targetId)
        {
            var currentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return currentId == targetId.ToString();
        }
    }
}

