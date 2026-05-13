using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IUserManagementService
    {
        Task<List<UserManagementDto>> GetUsersForManagementAsync();
        Task<(bool Success, string Message)> AddUserCampusAccessAsync(int userId, int campusId);
        Task<(bool Success, string Message)> RemoveUserCampusAccessAsync(int userId, int campusId);
        Task<(bool Success, string Message)> ToggleUserCampusBlockAsync(int userId, int campusId);
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto user);
        Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto user);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserITAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserAPAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserMessageAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserLibraryAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserHomeEconomicsAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserConsumablesAccessAsync(int userId);
    }

    public class UserManagementService : BaseService, IUserManagementService
    {
        public UserManagementService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<UserManagementDto>> GetUsersForManagementAsync()
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Get, "api/Users");
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<UserManagementDto>>() ?? new()) : new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddUserCampusAccessAsync(int userId, int campusId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Users/{userId}/campuses", campusId);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Access granted" : "Failed to grant access");
            }
            catch { return (false, "Error granting access"); }
        }

        public async Task<(bool Success, string Message)> RemoveUserCampusAccessAsync(int userId, int campusId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Users/{userId}/campuses/{campusId}");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Access removed" : "Failed to remove access");
            }
            catch { return (false, "Error removing access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserCampusBlockAsync(int userId, int campusId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/campuses/{campusId}/toggle-block");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto user)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Users", user);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User created successfully" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error creating user"); }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto user)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}", user);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User updated successfully" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error updating user"); }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Users/{userId}");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User deleted successfully" : "Failed to delete user");
            }
            catch { return (false, "Error deleting user"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-status");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserITAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-it-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "IT access toggled" : "Failed to toggle IT access");
            }
            catch { return (false, "Error toggling IT access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserAPAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-ap-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel access toggled" : "Failed to toggle Apparel access");
            }
            catch { return (false, "Error toggling Apparel access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserMessageAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-message-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Message access toggled" : "Failed to toggle message access");
            }
            catch { return (false, "Error toggling message access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserLibraryAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-library-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Library access toggled" : "Failed to toggle library access");
            }
            catch { return (false, "Error toggling library access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserHomeEconomicsAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-he-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Furniture access toggled" : "Failed to toggle Furniture access");
            }
            catch { return (false, "Error toggling HE access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserConsumablesAccessAsync(int userId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Users/{userId}/toggle-consumables-access");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Consumables access toggled" : "Failed to toggle Consumables access");
            }
            catch { return (false, "Error toggling consumables access"); }
        }
    }
}
