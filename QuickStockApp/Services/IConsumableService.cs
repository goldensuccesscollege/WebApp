using QuickStockApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
                if (campusId.HasValue && campusId.Value > 0)
                {
                    url += $"?campusId={campusId.Value}";
                }

                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConsumableResponse>>() ?? new List<ConsumableResponse>();
                }
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
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Consumable successfully created." : "Failed to create consumable.");
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
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Stock levels successfully augmented." : "Failed to update stock.");
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
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Stock inventory levels successfully reduced." : "Failed to deduct inventory.");
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
                var url = "api/ConsumableUnits/requests?";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrEmpty(status)) url += $"status={status}&";

                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConsumableRequestDto>>() ?? new List<ConsumableRequestDto>();
                }
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
    }
}
