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
        Task<(bool Success, string Message)> CreatePostAsync(string jwtToken, string content, List<(Stream Stream, string FileName)> images);
        Task<List<PostResponseDto>> GetUserPostsAsync(string jwtToken, string username);
        Task<bool> DeletePostAsync(string jwtToken, int postId);
        Task<List<string>> GetUserPhotosAsync(string jwtToken, string username);
        Task<(bool Success, bool Liked)> ToggleReactionAsync(string jwtToken, int postId);
        Task<CommentDto?> AddCommentAsync(string jwtToken, int postId, string content);
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

        public async Task<(bool Success, string Message)> CreatePostAsync(string jwtToken, string content, List<(Stream Stream, string FileName)> images)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(content), "Content");
                foreach (var img in images)
                {
                    var streamContent = new StreamContent(img.Stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formData.Add(streamContent, "Images", img.FileName);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "api/Profile/posts");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
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
