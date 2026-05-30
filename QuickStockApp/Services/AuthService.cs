using QuickStockApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IAuthService
    {
        Task<(LoginResponseDto? Result, string? Message)> LoginAsync(LoginRequestDto login);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest register);
        Task<(bool Success, string Message)> VerifyAsync(string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> CheckResetTokenAsync(string email, string token);
    }

    public class AuthService : BaseService, IAuthService
    {
        public AuthService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<(LoginResponseDto? Result, string? Message)> LoginAsync(LoginRequestDto login)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/Auth/login", login);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    return (result, null);
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<ApiErrorResponse>(errorMsg, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (null, errorObj?.Message ?? "Invalid credentials.");
                }
                catch
                {
                    return (null, errorMsg.Length > 100 ? errorMsg.Substring(0, 100) : errorMsg);
                }
            }
            catch (Exception ex)
            {
                return (null, $"Connection error: {ex.Message}");
            }
        }

        private class ApiErrorResponse { public string Message { get; set; } = ""; }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest register)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Auth/register", register);
                var msg = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(msg))
                    msg = response.IsSuccessStatusCode ? "Registration successful" : "Registration failed";
                return (response.IsSuccessStatusCode, msg);
            }
            catch
            {
                return (false, "Server error. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> VerifyAsync(string token)
        {
            try
            {
                var response = await _http.GetAsync($"api/Auth/verify?token={token}");
                var msg = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, string.IsNullOrEmpty(msg) ? "Verification failed" : msg);
                return (true, string.IsNullOrEmpty(msg) ? "Verification successful" : msg);
            }
            catch
            {
                return (false, "Server error. Please try again.");
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Auth/forgot-password", new { Email = email });
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var data = new { Email = email, Token = token, NewPassword = newPassword };
                var response = await _http.PostAsJsonAsync("api/Auth/reset-password", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> CheckResetTokenAsync(string email, string token)
        {
            try
            {
                var response = await _http.GetAsync(
                    $"api/Auth/check-reset-token?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
                if (!response.IsSuccessStatusCode) return false;
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            catch { return false; }
        }
    }
}