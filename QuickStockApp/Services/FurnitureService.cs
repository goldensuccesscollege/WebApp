using QuickStockApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IFurnitureService
    {
        Task<List<FurnitureDto>> GetFurnituresAsync(int? roomId = null, int? campusId = null, string? searchTerm = null);
        Task<(bool Success, string Message)> AddFurnitureAsync(FurnitureDto furniture);
        Task<(bool Success, string Message)> UpdateFurnitureAsync(FurnitureDto furniture);
        Task<(bool Success, string Message)> TransferFurnitureAsync(int id, int targetRoomId);
    }

    public class FurnitureService : BaseService, IFurnitureService
    {
        public FurnitureService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<FurnitureDto>> GetFurnituresAsync(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            try
            {
                var url = "api/Furniture?";
                if (roomId.HasValue && roomId.Value > 0) url += $"roomId={roomId.Value}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";

                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<FurnitureDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddFurnitureAsync(FurnitureDto furniture)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Furniture", furniture);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Furniture added successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateFurnitureAsync(FurnitureDto furniture)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, "api/Furniture", furniture);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Furniture updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> TransferFurnitureAsync(int id, int targetRoomId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Furniture/transfer", new { Id = id, TargetRoomId = targetRoomId });
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Furniture transferred successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
