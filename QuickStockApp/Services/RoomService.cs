using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetRoomsAsync(int? campusId = null);
        Task<(bool Success, string Message)> AddRoomAsync(RoomDto room);
        Task<(bool Success, string Message)> UpdateRoomAsync(RoomDto room);
        Task<(bool Success, string Message)> DeleteRoomAsync(int roomId);
        Task<(bool Success, string Message)> ToggleRoomStatusAsync(int roomId);
    }

    public class RoomService : BaseService, IRoomService
    {
        public RoomService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<RoomDto>> GetRoomsAsync(int? campusId = null)
        {
            try
            {
                var url = "api/Rooms";
                if (campusId.HasValue && campusId.Value > 0) url = $"api/Rooms?campusId={campusId.Value}";
                
                var request = await CreateRequestAsync(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<RoomDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddRoomAsync(RoomDto room)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Rooms", room);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Room added successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateRoomAsync(RoomDto room)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Rooms/{room.RoomId}", room);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Room updated successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> DeleteRoomAsync(int roomId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Rooms/{roomId}");
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Room deleted successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> ToggleRoomStatusAsync(int roomId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Rooms/{roomId}/toggle-status");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }
    }
}
