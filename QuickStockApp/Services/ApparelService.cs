using QuickStockApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IApparelService
    {
        Task<PaginatedApparelDto> GetApparelAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5);
        Task<PaginatedApparelItemDto> GetSoldItemsAsync(int? campusId = null, int page = 1, int pageSize = 10);
        Task<(bool Success, string Message)> AddApparelAsync(ApparelDto apparel);
        Task<(bool Success, string Message)> UpdateApparelAsync(ApparelDto apparel);
        Task<(bool Success, string Message)> DeleteApparelAsync(int id);
        Task<List<ApparelItemDto>> GetApparelItemsAsync(int apparelId);
        Task<(bool Success, string Message)> UpdateApparelItemStatusAsync(int itemId, string status);
        Task<(bool Success, string Message)> AddApparelStockAsync(int apparelId, int quantity);
        Task<List<ApparelItemDto>> QueryApparelItemsAsync(string? status, DateTime? startDate, DateTime? endDate, int? campusId);
    }

    public class ApparelService : BaseService, IApparelService
    {
        public ApparelService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<PaginatedApparelDto> GetApparelAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5)
        {
            try
            {
                var url = $"api/Apparel?page={page}&pageSize={pageSize}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedApparelDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<PaginatedApparelItemDto> GetSoldItemsAsync(int? campusId = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var url = $"api/Apparel/sold?page={page}&pageSize={pageSize}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedApparelItemDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddApparelAsync(ApparelDto apparel)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Apparel", apparel);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel added" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error adding apparel"); }
        }

        public async Task<(bool Success, string Message)> UpdateApparelAsync(ApparelDto apparel)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Apparel/{apparel.Apparel_ID}", apparel);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel updated" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error updating apparel"); }
        }

        public async Task<(bool Success, string Message)> DeleteApparelAsync(int id)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Apparel/{id}");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel deleted" : "Failed to delete apparel");
            }
            catch { return (false, "Error deleting apparel"); }
        }

        public async Task<List<ApparelItemDto>> GetApparelItemsAsync(int apparelId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Apparel/items/{apparelId}");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ApparelItemDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> UpdateApparelItemStatusAsync(int itemId, string status)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Apparel/item/status?itemId={itemId}&status={status}");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Status updated.");
                return (false, "Failed to update status.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> AddApparelStockAsync(int apparelId, int quantity)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Apparel/{apparelId}/add-stock", quantity);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Stock added.");
                return (false, "Failed to add stock.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<List<ApparelItemDto>> QueryApparelItemsAsync(string? status, DateTime? startDate, DateTime? endDate, int? campusId)
        {
            try
            {
                var url = "api/Apparel/items/query?";
                if (!string.IsNullOrEmpty(status)) url += $"status={status}&";
                if (startDate.HasValue) url += $"startDate={startDate.Value:yyyy-MM-dd}&";
                if (endDate.HasValue) url += $"endDate={endDate.Value:yyyy-MM-dd}&";
                if (campusId.HasValue) url += $"campusId={campusId}&";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ApparelItemDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }
    }
}
