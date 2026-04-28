using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class PublicProfileModel : PageModel
    {
        private readonly IApiService _api;
        private readonly IConfiguration _config;

        public PublicProfileModel(IApiService api, IConfiguration config)
        {
            _api = api;
            _config = config;
            ApiBaseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        }

        public string? ApiBaseUrl { get; }
        public ProfileDto? PublicProfile { get; set; }
        public List<PostResponseDto> Posts { get; set; } = new();
        public List<string> Photos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Index");

            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Account/Login");

            PublicProfile = await _api.GetPublicProfileAsync(token, username);
            if (PublicProfile == null)
            {
                ViewData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            Posts = await _api.GetUserPostsAsync(token, username);
            Photos = await _api.GetUserPhotosAsync(token, username);

            return Page();
        }


        public async Task<IActionResult> OnPostToggleLikeAsync([FromQuery] int postId)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var (success, liked) = await _api.ToggleReactionAsync(token, postId);
            return new JsonResult(new { success, liked });
        }

        public async Task<IActionResult> OnPostAddCommentAsync([FromQuery] int postId, [FromQuery] string content)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var comment = await _api.AddCommentAsync(token, postId, content);
            if (comment != null)
                return new JsonResult(new { success = true, comment });

            return new JsonResult(new { success = false });
        }
    }
}
