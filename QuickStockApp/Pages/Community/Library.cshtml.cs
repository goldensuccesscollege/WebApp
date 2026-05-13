using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;
using QuickStockApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace QuickStockApp.Pages.Community
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class LibraryModel : PageModel
    {
        private readonly IApiService _apiService;

        public LibraryModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<LibraryBookDto> Books { get; set; } = new();
        public List<CampusDto> AssignedCampuses { get; set; } = new();

        [BindProperty]
        public LibraryBookDto Book { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
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

            Books = await _apiService.GetLibraryBooksAsync(filterCampusId);

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
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager") && !User.IsInRole("User")) return Forbid();

            if (Book.CampusId <= 0)
            {
                var activeCampus = User.FindFirst("ActiveCampusId")?.Value;
                if (int.TryParse(activeCampus, out int acid))
                {
                    Book.CampusId = acid;
                }
            }

            var result = await _apiService.AddLibraryBookAsync(Book);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Book added successfully!";
                return RedirectToPage();
            }
            
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var isAnyAdmin = User.IsInRole("Admin") || User.IsInRole("Library Admin");
            if (!isAnyAdmin && !User.IsInRole("Manager")) return Forbid();

            var result = await _apiService.UpdateLibraryBookAsync(Book);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Book updated successfully!";
                return RedirectToPage();
            }
            TempData["ErrorMessage"] = result.Message;

            return RedirectToPage();
        }
    }
}
