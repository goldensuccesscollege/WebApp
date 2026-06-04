using QuickStockApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json; // 💡 Required to enable response.Content.ReadFromJsonAsync<T>()
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickStockApp.Services
{
    public interface IConsumableService
    {
        Task<List<ConsumableResponse>> GetAllConsumablesAsync(int? campusId = null);
        Task<(bool Success, string Message)> CreateConsumableAsync(CreateConsumableCommand command);
        Task<(bool Success, string Message)> AddStockAsync(AddStockCommand command);
        Task<(bool Success, string Message)> DeductStockAsync(DeductStockCommand command);
        Task<List<ConsumableRequestDto>> GetConsumableRequestsAsync(int? campusId = null, string? status = null);
        Task<(bool Success, string Message)> CreateConsumableRequestAsync(CreateConsumableRequestCommand command);
        Task<(bool Success, string Message)> ApproveConsumableRequestAsync(int id);
        Task<(bool Success, string Message)> RejectConsumableRequestAsync(int id, RejectConsumableRequestCommand command);
        
        // 🛠️ Interface Contract
        Task<List<ConsumableLedgerEntryDto>> GetConsumableLedgerAsync(int? campusId = null, int? productId = null);
    }

    public class ConsumableService : BaseService, IConsumableService
    {
        public ConsumableService(HttpClient http, IHttpContextAccessor httpContextAccessor)
            : base(http, httpContextAccessor) { }

        public async Task<List<ConsumableResponse>> GetAllConsumablesAsync(int? campusId = null)
        {
            try
            {
                var url = "api/ConsumableUnits";
                if (campusId.HasValue) url += $"?campusId={campusId.Value}";
                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<ConsumableResponse>>() ?? new List<ConsumableResponse>();
                return new List<ConsumableResponse>();
            }
            catch
            {
                return new List<ConsumableResponse>();
            }
        }

        public async Task<(bool Success, string Message)> CreateConsumableAsync(CreateConsumableCommand command)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/ConsumableUnits", command);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Inventory creation request submitted. Waiting for administrative review/approval." : "Failed to create consumable.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> AddStockAsync(AddStockCommand command)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/ConsumableUnits/add-stock", command);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Stock addition request submitted. Waiting for administrative review/approval." : "Failed to add stock.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeductStockAsync(DeductStockCommand command)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/ConsumableUnits/deduct-stock", command);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Stock deduction request submitted successfully. Waiting for review." : "Failed to deduct stock.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<ConsumableRequestDto>> GetConsumableRequestsAsync(int? campusId = null, string? status = null)
        {
            try
            {
                var url = "api/ConsumableUnits/requests";
                var parameters = new List<string>();
                if (campusId.HasValue) parameters.Add($"campusId={campusId.Value}");
                if (!string.IsNullOrEmpty(status)) parameters.Add($"status={status}");
                if (parameters.Count > 0) url += "?" + string.Join("&", parameters);

                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<ConsumableRequestDto>>() ?? new List<ConsumableRequestDto>();
                return new List<ConsumableRequestDto>();
            }
            catch
            {
                return new List<ConsumableRequestDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateConsumableRequestAsync(CreateConsumableRequestCommand command)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/ConsumableUnits/requests", command);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Request submitted successfully. Waiting for review." : "Failed to submit request.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> ApproveConsumableRequestAsync(int id)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/ConsumableUnits/requests/{id}/approve");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Request approved successfully." : "Failed to approve request.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> RejectConsumableRequestAsync(int id, RejectConsumableRequestCommand command)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/ConsumableUnits/requests/{id}/reject", command);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Request rejected successfully." : "Failed to reject request.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // 🛠️ Concrete Implementation of the Ledger API Connector Method
        public async Task<List<ConsumableLedgerEntryDto>> GetConsumableLedgerAsync(int? campusId = null, int? productId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (campusId.HasValue) queryParams.Add($"campusId={campusId.Value}");
                if (productId.HasValue) queryParams.Add($"productId={productId.Value}");
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"api/ConsumableUnits/ledger{queryString}";

                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConsumableLedgerEntryDto>>() 
                           ?? new List<ConsumableLedgerEntryDto>();
                }
                
                return new List<ConsumableLedgerEntryDto>();
            }
            catch
            {
                return new List<ConsumableLedgerEntryDto>();
            }
        }
    }
}