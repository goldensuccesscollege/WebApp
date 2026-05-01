using QuickStockApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient http, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<List<ItAssetDto>> GetItAssetsAsync(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var url = "api/Itassets?";
                if (roomId.HasValue && roomId.Value > 0) url += $"roomId={roomId.Value}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url.TrimEnd('&', '?'));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ItAssetDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddItAssetAsync(ItAssetDto asset)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Itassets")
                {
                    Content = JsonContent.Create(asset)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset added successfully");
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateItAssetAsync(ItAssetDto asset)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Itassets/{asset.Id}")
                {
                    Content = JsonContent.Create(asset)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> DeleteItAssetAsync(int id)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Itassets/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Asset deleted successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<List<RoomDto>> GetRoomsAsync(int? campusId = null)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var url = "api/Rooms";
                if (campusId.HasValue && campusId.Value > 0) url = $"api/Rooms?campusId={campusId.Value}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Rooms")
                {
                    Content = JsonContent.Create(room)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Rooms/{room.RoomId}")
                {
                    Content = JsonContent.Create(room)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Rooms/{roomId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Room deleted successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Campuses")
                {
                    Content = JsonContent.Create(campus)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Campuses/{campus.CampusId}")
                {
                    Content = JsonContent.Create(campus)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

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
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Campuses/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Campus deleted successfully.");
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> ToggleRoomStatusAsync(int roomId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Rooms/{roomId}/toggle-status");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }

        public async Task<List<UserManagementDto>> GetUsersForManagementAsync()
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, "api/Users");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<UserManagementDto>>() ?? new()) : new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddUserCampusAccessAsync(int userId, int campusId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Users/{userId}/campuses")
                {
                    Content = JsonContent.Create(campusId)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Access granted" : "Failed to grant access");
            }
            catch { return (false, "Error granting access"); }
        }

        public async Task<(bool Success, string Message)> RemoveUserCampusAccessAsync(int userId, int campusId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Users/{userId}/campuses/{campusId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Access removed" : "Failed to remove access");
            }
            catch { return (false, "Error removing access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserCampusBlockAsync(int userId, int campusId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}/campuses/{campusId}/toggle-block");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto user)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Users")
                {
                    Content = JsonContent.Create(user)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User created successfully" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error creating user"); }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto user)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}")
                {
                    Content = JsonContent.Create(user)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User updated successfully" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error updating user"); }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Users/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "User deleted successfully" : "Failed to delete user");
            }
            catch { return (false, "Error deleting user"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}/toggle-status");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Status toggled" : "Failed to toggle status");
            }
            catch { return (false, "Error toggling status"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserITAccessAsync(int userId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}/toggle-it-access");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "IT access toggled" : "Failed to toggle IT access");
            }
            catch { return (false, "Error toggling IT access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserAPAccessAsync(int userId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}/toggle-ap-access");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel access toggled" : "Failed to toggle Apparel access");
            }
            catch { return (false, "Error toggling Apparel access"); }
        }

        public async Task<(bool Success, string Message)> ToggleUserMessageAccessAsync(int userId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Users/{userId}/toggle-message-access");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Message access toggled" : "Failed to toggle message access");
            }
            catch { return (false, "Error toggling message access"); }
        }

        public async Task<(DashboardDto? Stats, string Message)> GetDashboardStatsAsync(int campusId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Dashboard/{campusId}/stats");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
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
                var jwt = await GetTokenAsync();
                var url = "api/AuditLogs?";
                if (campusId.HasValue) url += $"campusId={campusId}&";
                if (!string.IsNullOrEmpty(entityType)) url += $"entityType={entityType}&";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url.TrimEnd('&', '?'));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
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
                var jwt = await GetTokenAsync();
                var url = $"api/AuditLogs?page={page}&pageSize={pageSize}";
                if (campusId.HasValue && campusId.Value > 0) url += $"&campusId={campusId}";
                if (!string.IsNullOrEmpty(entityType)) url += $"&entityType={entityType}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedAuditLogsDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> TransferItAssetAsync(int assetId, int targetRoomId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/itassets/{assetId}/transfer");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                request.Content = JsonContent.Create(targetRoomId);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Transfer successful" : "Failed to transfer asset");
            }
            catch { return (false, "Error during asset transfer"); }
        }

        public async Task<PaginatedApparelDto> GetApparelAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var url = $"api/Apparel?page={page}&pageSize={pageSize}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url.TrimEnd('&', '?'));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedApparelDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<PaginatedApparelItemDto> GetSoldItemsAsync(int? campusId = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var url = $"api/Apparel/sold?page={page}&pageSize={pageSize}&";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}&";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url.TrimEnd('&', '?'));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaginatedApparelItemDto>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddApparelAsync(ApparelDto apparel)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, "api/Apparel")
                {
                    Content = JsonContent.Create(apparel)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel added" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error adding apparel"); }
        }

        public async Task<(bool Success, string Message)> UpdateApparelAsync(ApparelDto apparel)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Put, $"api/Apparel/{apparel.Apparel_ID}")
                {
                    Content = JsonContent.Create(apparel)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel updated" : await response.Content.ReadAsStringAsync());
            }
            catch { return (false, "Error updating apparel"); }
        }

        public async Task<(bool Success, string Message)> DeleteApparelAsync(int id)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Apparel/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Apparel deleted" : "Failed to delete apparel");
            }
            catch { return (false, "Error deleting apparel"); }
        }

        public async Task<List<ApparelItemDto>> GetApparelItemsAsync(int apparelId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Apparel/items/{apparelId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ApparelItemDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> UpdateApparelItemStatusAsync(int itemId, string status)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Apparel/item/status?itemId={itemId}&status={status}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Status updated.");
                return (false, "Failed to update status.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> AddApparelStockAsync(int apparelId, int quantity)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Apparel/{apparelId}/add-stock");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                request.Content = JsonContent.Create(quantity);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Stock added.");
                return (false, "Failed to add stock.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<List<ApparelItemDto>> QueryApparelItemsAsync(string? status, DateTime? startDate, DateTime? endDate, int? campusId)
        {
            try
            {
                var jwt = await GetTokenAsync();
                var url = "api/Apparel/items/query?";
                if (!string.IsNullOrEmpty(status)) url += $"status={status}&";
                if (startDate.HasValue) url += $"startDate={startDate.Value:yyyy-MM-dd}&";
                if (endDate.HasValue) url += $"endDate={endDate.Value:yyyy-MM-dd}&";
                if (campusId.HasValue) url += $"campusId={campusId}&";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url.TrimEnd('&', '?'));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
                
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ApparelItemDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        private Task<string?> GetTokenAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext?.User?.FindFirst("jwt_token")?.Value);
        }
    }
}
