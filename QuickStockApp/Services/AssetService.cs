using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IAssetService
    {
        Task<List<ItAssetDto>> GetItAssetsAsync(int? roomId = null, int? campusId = null, string? searchTerm = null);
        Task<(bool Success, string Message)> AddItAssetAsync(ItAssetDto asset);
        Task<(bool Success, string Message)> UpdateItAssetAsync(ItAssetDto asset);
        Task<(bool Success, string Message)> DeleteItAssetAsync(int id);
        Task<(bool Success, string Message)> TransferItAssetAsync(int assetId, int targetRoomId);
    }

    public class AssetService : BaseService, IAssetService
    {
        public AssetService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<ItAssetDto>> GetItAssetsAsync(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            try
            {
                var url = "api/Itassets?";
                if (roomId.HasValue && roomId.Value > 0) url += $"roomId={roomId.Value}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ItAssetDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddItAssetAsync(ItAssetDto asset)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Itassets", asset);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset added successfully");
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateItAssetAsync(ItAssetDto asset)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Itassets/{asset.Id}", asset);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> DeleteItAssetAsync(int id)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Itassets/{id}");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset deleted successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> TransferItAssetAsync(int assetId, int targetRoomId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/itassets/{assetId}/transfer", targetRoomId);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Transfer successful" : "Failed to transfer asset");
            }
            catch { return (false, "Error during asset transfer"); }
        }
    }
}
