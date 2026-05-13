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
        public FurnitureDto? Furniture { get; set; }
        public string? ErrorMessage { get; set; }


        public async Task<IActionResult> OnGetAsync(string? qr)
        {
            if (string.IsNullOrEmpty(qr))
            {
                ErrorMessage = "Invalid QR code.";
                return Page();
            }

            // Extract code if a full URL was scanned
            if (qr.Contains("/Scan/Details?qr="))
            {
                try
                {
                    if (qr.Contains("?"))
                    {
                        var queryString = qr.Split('?')[1];
                        var parts = queryString.Split('&');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("qr="))
                            {
                                qr = Uri.UnescapeDataString(part.Substring(3));
                                break;
                            }
                        }
                    }
                }
                catch { /* Not a valid URL, treat as literal code */ }
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

                // Try Furniture
                var furnitureResponse = await _http.GetAsync($"{baseUrl}/api/furniture/qr/{qr}");
                if (furnitureResponse.IsSuccessStatusCode)
                {
                    var json = await furnitureResponse.Content.ReadAsStringAsync();
                    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Furniture = System.Text.Json.JsonSerializer.Deserialize<FurnitureDto>(json, opts);

                    if (Furniture != null)
                    {
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(json);
                            var root = doc.RootElement;

                            // Fallback for underscore names if normal deserialization missed them
                            if (string.IsNullOrEmpty(Furniture.Item_Name))
                                Furniture.Item_Name = root.TryGetProperty("itemName", out var itn) ? itn.GetString() ?? "" : "";
                            
                            if (string.IsNullOrEmpty(Furniture.Item_number))
                                Furniture.Item_number = root.TryGetProperty("itemNumber", out var itnu) ? itnu.GetString() : null;

                            if (Furniture.Item_count == 0 || Furniture.Item_count == 1)
                                if (root.TryGetProperty("itemCount", out var itc)) Furniture.Item_count = itc.GetInt32();

                            // Extract nested Room and Campus names
                            if (root.TryGetProperty("room", out var roomEl) && roomEl.ValueKind != System.Text.Json.JsonValueKind.Null)
                                Furniture.RoomName = roomEl.TryGetProperty("roomName", out var rn) ? rn.GetString() : null;

                            if (root.TryGetProperty("campus", out var campusEl) && campusEl.ValueKind != System.Text.Json.JsonValueKind.Null)
                                Furniture.CampusName = campusEl.TryGetProperty("name", out var cn) ? cn.GetString() : null;

                            if (root.TryGetProperty("totalItemsInRoom", out var totalEl))
                                Furniture.TotalItemsInRoom = totalEl.GetInt32();
                        }
                        catch { /* ignore parsing errors */ }
                    }           
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
