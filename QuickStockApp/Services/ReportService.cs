using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IReportService
    {
        Task<(DashboardDto? Stats, string Message)> GetDashboardStatsAsync(int campusId);
        Task<List<AuditLogDto>> GetAuditLogsAsync(int? campusId = null, string? entityType = null);
        Task<PaginatedAuditLogsDto> GetAuditLogsPaginatedAsync(int? campusId, int page, int pageSize, string? entityType = null);
    }

    public class ReportService : BaseService, IReportService
    {
        public ReportService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<(DashboardDto? Stats, string Message)> GetDashboardStatsAsync(int campusId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, $"api/Dashboard/{campusId}/stats");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var stats = await response.Content.ReadFromJsonAsync<DashboardDto>();
                    return (stats, "Success");
                }
                return (null, "Failed to fetch stats");
            }
            catch { return (null, "Error fetching dashboard stats"); }
        }

        public async Task<List<AuditLogDto>> GetAuditLogsAsync(int? campusId = null, string? entityType = null)
        {
            try
            {
                var url = "api/AuditLogs?";
                if (campusId.HasValue) url += $"campusId={campusId}&";
                if (!string.IsNullOrEmpty(entityType)) url += $"entityType={entityType}&";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('&', '?'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<PaginatedAuditLogsDto>();
                    return data?.Logs ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<PaginatedAuditLogsDto> GetAuditLogsPaginatedAsync(int? campusId, int page, int pageSize, string? entityType = null)
        {
            try
            {
                var url = $"api/AuditLogs?page={page}&pageSize={pageSize}";
                if (campusId.HasValue && campusId.Value > 0) url += $"&campusId={campusId}";
                if (!string.IsNullOrEmpty(entityType)) url += $"&entityType={entityType}";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedAuditLogsDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }
    }
}
