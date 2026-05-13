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
        Task<List<ItAssetDto>> GetItAssetsAsync(int? roomId = null, int? campusId = null, string? searchTerm = null);
        Task<(bool Success, string Message)> AddItAssetAsync(ItAssetDto asset);
        Task<(bool Success, string Message)> UpdateItAssetAsync(ItAssetDto asset);
        Task<(bool Success, string Message)> DeleteItAssetAsync(int id);
        Task<List<RoomDto>> GetRoomsAsync(int? campusId = null);
        Task<(bool Success, string Message)> AddRoomAsync(RoomDto room);
        Task<(bool Success, string Message)> UpdateRoomAsync(RoomDto room);
        Task<(bool Success, string Message)> DeleteRoomAsync(int roomId);

        Task<List<CampusDto>> GetCampusesAsync(string? token = null);
        Task<(bool Success, string Message)> AddCampusAsync(CampusDto campus);
        Task<(bool Success, string Message)> UpdateCampusAsync(CampusDto campus);
        Task<(bool Success, string Message)> DeleteCampusAsync(int id);

        Task<(bool Success, string Message)> ToggleRoomStatusAsync(int roomId);
        Task<List<UserManagementDto>> GetUsersForManagementAsync();
        Task<(bool Success, string Message)> AddUserCampusAccessAsync(int userId, int campusId);
        Task<(bool Success, string Message)> RemoveUserCampusAccessAsync(int userId, int campusId);
        Task<(bool Success, string Message)> ToggleUserCampusBlockAsync(int userId, int campusId);
        
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto user);
        Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto user);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserITAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserAPAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserMessageAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserLibraryAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserHomeEconomicsAccessAsync(int userId);
        Task<(bool Success, string Message)> ToggleUserConsumablesAccessAsync(int userId);
        
        Task<(DashboardDto? Stats, string Message)> GetDashboardStatsAsync(int campusId);
        Task<List<AuditLogDto>> GetAuditLogsAsync(int? campusId = null, string? entityType = null);
        Task<PaginatedAuditLogsDto> GetAuditLogsPaginatedAsync(int? campusId, int page, int pageSize, string? entityType = null);
        Task<(bool Success, string Message)> TransferItAssetAsync(int assetId, int targetRoomId);

        // Apparel
        Task<PaginatedApparelDto> GetApparelAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5);
        Task<PaginatedApparelItemDto> GetSoldItemsAsync(int? campusId = null, int page = 1, int pageSize = 10);
        Task<(bool Success, string Message)> AddApparelAsync(ApparelDto apparel);
        Task<(bool Success, string Message)> UpdateApparelAsync(ApparelDto apparel);
        Task<(bool Success, string Message)> DeleteApparelAsync(int id);
        Task<List<ApparelItemDto>> GetApparelItemsAsync(int apparelId);
        Task<(bool Success, string Message)> UpdateApparelItemStatusAsync(int itemId, string status);
        Task<(bool Success, string Message)> AddApparelStockAsync(int apparelId, int quantity);
        Task<List<ApparelItemDto>> QueryApparelItemsAsync(string? status, DateTime? startDate, DateTime? endDate, int? campusId);

        // Library
        Task<List<LibraryBookDto>> GetLibraryBooksAsync(int? campusId = null);
        Task<(bool Success, string Message)> AddLibraryBookAsync(LibraryBookDto book);
        Task<(bool Success, string Message)> UpdateLibraryBookAsync(LibraryBookDto book);

        // Library Items
        Task<(bool Success, string Message)> AddLibraryBookItemAsync(int bookId, LibraryBookItemDto item);
        Task<(bool Success, string Message)> UpdateLibraryBookItemAsync(int itemId, LibraryBookItemDto item);
        Task<(bool Success, string Message)> DeleteLibraryBookItemAsync(int itemId);

        // Furniture
        Task<List<FurnitureDto>> GetFurnituresAsync(int? roomId = null, int? campusId = null, string? searchTerm = null);
        Task<(bool Success, string Message)> AddFurnitureAsync(FurnitureDto furniture);
        Task<(bool Success, string Message)> UpdateFurnitureAsync(FurnitureDto furniture);
        Task<(bool Success, string Message)> TransferFurnitureAsync(int id, int targetRoomId);

        // Consumables
        Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId);
        Task<(bool Success, string Message)> CreateConsumableAsync(ConsumableDto consumable);
        Task<bool> UpdateConsumableItemStatusAsync(int itemId, string status);
    }
}


