using QuickStockApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(string jwtToken);
        Task<(bool Success, string Message)> UpdateProfileAsync(string jwtToken, UpdateProfileRequest profile, Stream? imageStream, string? fileName);
        Task<ProfileDto?> GetPublicProfileAsync(string jwtToken, string username);
    }

    public class ProfileService : BaseService, IProfileService
    {
        public ProfileService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<ProfileDto?> GetProfileAsync(string jwtToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Profile/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProfileDto>() : null;
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
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "ImageProfile", fileName);
                }

                var request = new HttpRequestMessage(HttpMethod.Put, "api/Profile/update");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                request.Content = content;

                var response = await _http.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                
                try {
                    var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(result);
                    if (json.TryGetProperty("message", out var msg)) result = msg.GetString() ?? result;
                } catch {}

                return (response.IsSuccessStatusCode, result);
            }
            catch (Exception ex) { return (false, $"Error: {ex.Message}"); }
        }

        public async Task<ProfileDto?> GetPublicProfileAsync(string jwtToken, string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Profile/user/{username}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProfileDto>() : null;
            }
            catch { return null; }
        }
    }
}
