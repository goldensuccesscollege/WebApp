using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface ICampusService
    {
        Task<List<CampusDto>> GetCampusesAsync(string? token = null);
        Task<(bool Success, string Message)> AddCampusAsync(CampusDto campus);
        Task<(bool Success, string Message)> UpdateCampusAsync(CampusDto campus);
        Task<(bool Success, string Message)> DeleteCampusAsync(int id);
    }

    public class CampusService : BaseService, ICampusService
    {
        public CampusService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<CampusDto>> GetCampusesAsync(string? token = null)
        {
            try
            {
                var jwt = token ?? await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Campuses");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CampusDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddCampusAsync(CampusDto campus)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Campuses", campus);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Campus added successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateCampusAsync(CampusDto campus)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Campuses/{campus.CampusId}", campus);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Campus updated successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> DeleteCampusAsync(int id)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Campuses/{id}");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Campus deleted successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
