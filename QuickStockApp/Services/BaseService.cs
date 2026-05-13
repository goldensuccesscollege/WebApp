using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public abstract class BaseService
    {
        protected readonly HttpClient _http;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        protected async Task<string> GetTokenAsync()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value ?? string.Empty;
        }

        protected async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string url, object? content = null)
        {
            var jwt = await GetTokenAsync();
            var request = new HttpRequestMessage(method, url);
            
            if (!string.IsNullOrEmpty(jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }

            if (content != null)
            {
                if (content is HttpContent httpContent)
                {
                    request.Content = httpContent;
                }
                else
                {
                    request.Content = JsonContent.Create(content);
                }
            }

            return request;
        }

        protected async Task<T?> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return default;
        }
    }
}
