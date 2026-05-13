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
        Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId);
        Task<(bool Success, string Message)> CreateConsumableAsync(ConsumableDto consumable);
        Task<bool> UpdateItemStatusAsync(int itemId, string status);
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

        public async Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Consumables?campusId={campusId}&searchTerm={searchTerm}&page={page}&pageSize={pageSize}");
                var response = await _http.SendAsync(request);
                return await response.Content.ReadFromJsonAsync<ConsumableListResponse>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Consumables/items/{consumableId}");
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
    }
}
