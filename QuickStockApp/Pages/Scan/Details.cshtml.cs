using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages.Scan
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public DetailsModel(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public ItAssetDto? Asset { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? qr)
        {
            if (string.IsNullOrEmpty(qr))
            {
                ErrorMessage = "Invalid QR code.";
                return Page();
            }

            try
            {
                var baseUrl = _config["ApiSettings:BaseUrl"];
                var response = await _http.GetAsync($"{baseUrl}/api/itassets/qr/{qr}");

                if (response.IsSuccessStatusCode)
                {
                    Asset = await response.Content.ReadFromJsonAsync<ItAssetDto>();
                }
                else
                {
                    ErrorMessage = "Asset not found or QR code expired.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}
