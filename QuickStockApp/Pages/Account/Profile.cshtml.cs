using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Models;
using QuickStockApp.Services;
using Microsoft.Extensions.Configuration;

namespace QuickStockApp.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IApiService _api;
        private readonly IConfiguration _config;

        public ProfileModel(IApiService api, IConfiguration config)
        {
            _api = api;
            _config = config;
            ApiBaseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        }
        
        public string? ApiBaseUrl { get; }

        [BindProperty]
        public UpdateProfileRequest ProfileRequest { get; set; } = new();

        public ProfileDto? CurrentProfile { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public List<PostResponseDto> Posts { get; set; } = new();
        public List<string> Photos { get; set; } = new();

        [BindProperty]
        public string? PostContent { get; set; }

        public async Task<IActionResult> OnPostToggleLikeAsync([FromQuery] int postId)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var (success, liked) = await _api.ToggleReactionAsync(token, postId);
            
            return new JsonResult(new { success, liked });
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            CurrentProfile = await _api.GetProfileAsync(token);
            if (CurrentProfile == null)
            {
                ErrorMessage = "Could not load profile.";
                return Page();
            }

            // Map to request object
            ProfileRequest.FirstName = CurrentProfile.FirstName;
            ProfileRequest.LastName = CurrentProfile.LastName;
            ProfileRequest.Birthday = CurrentProfile.Birthday;
            ProfileRequest.Address = CurrentProfile.Address;
            ProfileRequest.PhoneNumber = CurrentProfile.PhoneNumber;

            Posts = await _api.GetUserPostsAsync(token, CurrentProfile.Username);
            Photos = await _api.GetUserPhotosAsync(token, CurrentProfile.Username);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? ImageProfile)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            Stream? imageStream = null;
            string? fileName = null;

            if (ImageProfile != null)
            {
                imageStream = ImageProfile.OpenReadStream();
                fileName = ImageProfile.FileName;
            }

            var (success, message) = await _api.UpdateProfileAsync(token, ProfileRequest, imageStream, fileName);

            if (success) SuccessMessage = "Profile updated successfully!";
            else ErrorMessage = message;

            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostCreatePostAsync(List<IFormFile> PostImages)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Account/Login");

            if (string.IsNullOrWhiteSpace(PostContent) && (PostImages == null || !PostImages.Any()))
            {
                ErrorMessage = "Please add some text or a photo to post.";
                return await OnGetAsync();
            }

            var imageFiles = new List<(Stream Stream, string FileName)>();
            if (PostImages != null)
            {
                foreach (var file in PostImages)
                {
                    imageFiles.Add((file.OpenReadStream(), file.FileName));
                }
            }

            var (success, message) = await _api.CreatePostAsync(token, PostContent ?? "", imageFiles);
            if (success) SuccessMessage = "Post shared!";
            else ErrorMessage = message;

            return await OnGetAsync();
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

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Account/Login");

            var success = await _api.DeletePostAsync(token, postId);
            if (success) SuccessMessage = "Post deleted.";
            
            return await OnGetAsync();
        }
    }
}
