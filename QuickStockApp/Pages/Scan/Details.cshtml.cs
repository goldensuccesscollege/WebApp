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
        public ApparelItemDto? ApparelItem { get; set; }
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
                
                // Try IT Asset first
                var assetResponse = await _http.GetAsync($"{baseUrl}/api/itassets/qr/{qr}");
                if (assetResponse.IsSuccessStatusCode)
                {
                    Asset = await assetResponse.Content.ReadFromJsonAsync<ItAssetDto>();
                    return Page();
                }

                // If not found, try Apparel Item
                var apparelResponse = await _http.GetAsync($"{baseUrl}/api/apparel/item/qr/{qr}");
                if (apparelResponse.IsSuccessStatusCode)
                {
                    ApparelItem = await apparelResponse.Content.ReadFromJsonAsync<ApparelItemDto>();
                    return Page();
                }

                ErrorMessage = "Record not found or QR code expired.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}
