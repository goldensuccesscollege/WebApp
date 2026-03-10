using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuickStockApp.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ChatModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ApiBaseUrl { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;

        public void OnGet()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7198";
            ApiBaseUrl = baseUrl.TrimEnd('/');
            JwtToken = User.FindFirst("jwt_token")?.Value ?? string.Empty;
        }
    }
}
