using QuickStockApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

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

                // Try to parse the error message from the API (matches ExceptionMiddleware)
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (null, error?.Message ?? "Invalid credentials.");
            }
            catch (Exception ex)
            {
                return (null, $"Connection error: {ex.Message}");
            }
        }

        private class ApiErrorResponse { public string Message { get; set; } = ""; }

        // IMPLEMENTATION for RegisterAsync
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

        // IMPLEMENTATION for VerifyAsync
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

        // --- Profile Implementation ---

        public async Task<ProfileDto?> GetProfileAsync(string jwtToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Profile/me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProfileDto>();
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(string jwtToken, UpdateProfileRequest profile, Stream? imageStream, string? fileName)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(profile.FirstName), "FirstName");
                content.Add(new StringContent(profile.LastName), "LastName");
                if (profile.Birthday.HasValue)
                    content.Add(new StringContent(profile.Birthday.Value.ToString("yyyy-MM-dd")), "Birthday");
                if (!string.IsNullOrEmpty(profile.Address))
                    content.Add(new StringContent(profile.Address), "Address");
                if (!string.IsNullOrEmpty(profile.PhoneNumber))
                    content.Add(new StringContent(profile.PhoneNumber), "PhoneNumber");

                if (imageStream != null && !string.IsNullOrEmpty(fileName))
                {
                    var fileContent = new StreamContent(imageStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // Default to jpeg
                    content.Add(fileContent, "ImageProfile", fileName);
                }

                var request = new HttpRequestMessage(HttpMethod.Put, "api/Profile/update");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                request.Content = content;

                var response = await _http.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                
                // Try to parse JSON message if it exists
                try {
                    var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(result);
                    if (json.TryGetProperty("message", out var msg)) result = msg.GetString() ?? result;
                } catch {}

                return (response.IsSuccessStatusCode, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<ProfileDto?> GetPublicProfileAsync(string jwtToken, string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Profile/user/{username}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProfileDto>() : null;
            }
            catch { return null; }
        }

        public async Task<(bool Success, string Message)> CreatePostAsync(string jwtToken, string content, List<(Stream Stream, string FileName)> images)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(content), "Content");
                foreach (var img in images)
                {
                    var streamContent = new StreamContent(img.Stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    formData.Add(streamContent, "Images", img.FileName);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "api/Profile/posts");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                request.Content = formData;

                var response = await _http.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, result);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<List<PostResponseDto>> GetUserPostsAsync(string jwtToken, string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Profile/posts/{username}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<PostResponseDto>>() ?? new()) : new();
            }
            catch { return new(); }
        }

        public async Task<bool> DeletePostAsync(string jwtToken, int postId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Profile/posts/{postId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<string>> GetUserPhotosAsync(string jwtToken, string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Profile/photos/{username}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<string>>() ?? new()) : new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, bool Liked)> ToggleReactionAsync(string jwtToken, int postId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Profile/posts/{postId}/react");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var liked = json.Contains("\"liked\":true") || json.Contains(":true");
                    return (true, liked);
                }
                return (false, false);
            }
            catch { return (false, false); }
        }

        public async Task<CommentDto?> AddCommentAsync(string jwtToken, int postId, string content)
        {
            try
            {
                var contentData = new MultipartFormDataContent();
                contentData.Add(new StringContent(content), "content");

                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Profile/posts/{postId}/comments");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                request.Content = contentData;

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CommentDto>();
                }
                return null;
            }
            catch { return null; }
        }
    }
}
