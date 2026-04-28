using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class VerifyModel : PageModel
    {
        private readonly IApiService _api;

        public VerifyModel(IApiService api)
        {
            _api = api;
        }

        public string ModalMessage { get; set; } = "";
        public bool ShowModal { get; set; } = false;
        public bool IsSuccess { get; set; } = false;

        public void OnGet()
        {
            // Just render the page, JS handles the rest
        }

        public async Task<IActionResult> OnGetVerifyJsonAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { success = false, message = "Token is required." });
            }

            var result = await _api.VerifyAsync(token);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }
    }
}
