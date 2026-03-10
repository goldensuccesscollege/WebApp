using System.Threading.Tasks;
using QuickStockApp.Models;
using System.Collections.Generic;
using System.IO;

namespace QuickStockApp.Services
{
    public interface IApiService
    {
        Task<(LoginResponseDto? Result, string? Message)> LoginAsync(LoginRequestDto login);
 
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest register);

        Task<(bool Success, string Message)> VerifyAsync(string token);

        Task<bool> ForgotPasswordAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> CheckResetTokenAsync(string email, string token);
        
        // Profile
        Task<ProfileDto?> GetProfileAsync(string jwtToken);
        Task<(bool Success, string Message)> UpdateProfileAsync(string jwtToken, UpdateProfileRequest request, Stream? imageStream, string? fileName);
        Task<ProfileDto?> GetPublicProfileAsync(string jwtToken, string username);
        Task<(bool Success, string Message)> CreatePostAsync(string jwtToken, string content, List<(Stream Stream, string FileName)> images);
        Task<List<PostResponseDto>> GetUserPostsAsync(string jwtToken, string username);
        Task<bool> DeletePostAsync(string jwtToken, int postId);
        Task<List<string>> GetUserPhotosAsync(string jwtToken, string username);
        Task<(bool Success, bool Liked)> ToggleReactionAsync(string jwtToken, int postId);
        Task<CommentDto?> AddCommentAsync(string jwtToken, int postId, string content);
    }
}
