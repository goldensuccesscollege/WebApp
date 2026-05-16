using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QuickStockApp.Models;

namespace QuickStockApp.Services
{
    public interface IConsumableService
    {
        Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10, bool showOutOnly = false);
        Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId, bool showOutOnly = false);
        Task<(bool Success, string Message)> CreateConsumableAsync(ConsumableDto consumable);
        Task<bool> UpdateItemStatusAsync(int itemId, string status);
        Task<List<ConsumableOutRequestDto>> GetPendingOutRequestsAsync(int campusId);
        Task<List<ConsumableOutRequestDto>> GetOutRequestHistoryAsync(int campusId, string? status = null, string? userId = null);
        Task<(bool Success, string Message)> RequestItemOutAsync(int itemId);
        Task<(bool Success, string Message)> ApproveOutRequestAsync(int requestId);
        Task<(bool Success, string Message)> RejectOutRequestAsync(int requestId, string reason);
        Task<(bool Success, string Message)> CancelOutRequestAsync(int requestId);
        Task<bool> UpdateConsumableAsync(ConsumableDto consumable);
        Task<bool> RestockConsumableAsync(int consumableId, int quantity);
    }

    public class ConsumableListResponse
    {
        public List<ConsumableDto> Consumables { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class ConsumableService : BaseService, IConsumableService
    {
        public ConsumableService(HttpClient http, IHttpContextAccessor httpContextAccessor) 
            : base(http, httpContextAccessor)
        {
        }

        public async Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10, bool showOutOnly = false)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Consumables?campusId={campusId}&searchTerm={searchTerm}&page={page}&pageSize={pageSize}&showOutOnly={showOutOnly}");
                var response = await _http.SendAsync(request);
                return await response.Content.ReadFromJsonAsync<ConsumableListResponse>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId, bool showOutOnly = false)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Consumables/items/{consumableId}?showOutOnly={showOutOnly}");
                var response = await _http.SendAsync(request);
                return await response.Content.ReadFromJsonAsync<List<ConsumableItemDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> CreateConsumableAsync(ConsumableDto consumable)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Consumables", consumable);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Success" : await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<bool> UpdateItemStatusAsync(int itemId, string status)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/item/status?itemId={itemId}&status={status}");
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<ConsumableOutRequestDto>> GetPendingOutRequestsAsync(int campusId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Consumables/out-requests?campusId={campusId}");
                var response = await _http.SendAsync(request);
                return await response.Content.ReadFromJsonAsync<List<ConsumableOutRequestDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<ConsumableOutRequestDto>> GetOutRequestHistoryAsync(int campusId, string? status = null, string? userId = null)
        {
            try
            {
                var url = $"api/Consumables/out-requests/history?campusId={campusId}";
                if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
                if (!string.IsNullOrEmpty(userId)) url += $"&userId={userId}";
                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                return await response.Content.ReadFromJsonAsync<List<ConsumableOutRequestDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> RequestItemOutAsync(int itemId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/item/request-out?itemId={itemId}");
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadFromJsonAsync<dynamic>();
                return (response.IsSuccessStatusCode, body?.GetProperty("message").GetString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> ApproveOutRequestAsync(int requestId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/out-requests/{requestId}/approve");
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadFromJsonAsync<dynamic>();
                return (response.IsSuccessStatusCode, body?.GetProperty("message").GetString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> RejectOutRequestAsync(int requestId, string reason)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/out-requests/{requestId}/reject", reason);
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadFromJsonAsync<dynamic>();
                return (response.IsSuccessStatusCode, body?.GetProperty("message").GetString() ?? "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> CancelOutRequestAsync(int requestId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/out-requests/{requestId}/cancel");
                var response = await _http.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Request cancelled." : "Failed to cancel request.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<bool> UpdateConsumableAsync(ConsumableDto consumable)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, "api/Consumables", consumable);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> RestockConsumableAsync(int consumableId, int quantity)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Consumables/{consumableId}/restock", quantity);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
