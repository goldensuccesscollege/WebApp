using QuickStockApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public class ApiService : IApiService
    {
        private readonly IAuthService _auth;
        private readonly IAssetService _assets;
        private readonly IRoomService _rooms;
        private readonly ICampusService _campuses;
        private readonly IReportService _reports;
        private readonly IApparelService _apparel;
        private readonly IFurnitureService _furniture;
        private readonly ILibraryService _library;
        private readonly IProfileService _profile;
        private readonly IUserManagementService _userMgmt;
        private readonly IConsumableService _consumables;

        public ApiService(
            IAuthService auth,
            IAssetService assets,
            IRoomService rooms,
            ICampusService campuses,
            IReportService reports,
            IApparelService apparel,
            IFurnitureService furniture,
            ILibraryService library,
            IProfileService profile,
            IUserManagementService userMgmt,
            IConsumableService consumables)
        {
            _auth = auth;
            _assets = assets;
            _rooms = rooms;
            _campuses = campuses;
            _reports = reports;
            _apparel = apparel;
            _furniture = furniture;
            _library = library;
            _profile = profile;
            _userMgmt = userMgmt;
            _consumables = consumables;
        }

        // Auth
        public Task<(LoginResponseDto? Result, string? Message)> LoginAsync(LoginRequestDto login) => _auth.LoginAsync(login);
        public Task<(bool Success, string Message)> RegisterAsync(RegisterRequest register) => _auth.RegisterAsync(register);
        public Task<(bool Success, string Message)> VerifyAsync(string token) => _auth.VerifyAsync(token);
        public Task<bool> ForgotPasswordAsync(string email) => _auth.ForgotPasswordAsync(email);
        public Task<bool> ResetPasswordAsync(string email, string token, string newPassword) => _auth.ResetPasswordAsync(email, token, newPassword);
        public Task<bool> CheckResetTokenAsync(string email, string token) => _auth.CheckResetTokenAsync(email, token);

        // Profile
        public Task<ProfileDto?> GetProfileAsync(string jwtToken) => _profile.GetProfileAsync(jwtToken);
        public Task<(bool Success, string Message)> UpdateProfileAsync(string jwtToken, UpdateProfileRequest request, Stream? imageStream, string? fileName) => _profile.UpdateProfileAsync(jwtToken, request, imageStream, fileName);
        public Task<ProfileDto?> GetPublicProfileAsync(string jwtToken, string username) => _profile.GetPublicProfileAsync(jwtToken, username);
        public Task<(bool Success, string Message)> CreatePostAsync(string jwtToken, string content, List<(Stream Stream, string FileName)> images) => _profile.CreatePostAsync(jwtToken, content, images);
        public Task<List<PostResponseDto>> GetUserPostsAsync(string jwtToken, string username) => _profile.GetUserPostsAsync(jwtToken, username);
        public Task<bool> DeletePostAsync(string jwtToken, int postId) => _profile.DeletePostAsync(jwtToken, postId);
        public Task<List<string>> GetUserPhotosAsync(string jwtToken, string username) => _profile.GetUserPhotosAsync(jwtToken, username);
        public Task<(bool Success, bool Liked)> ToggleReactionAsync(string jwtToken, int postId) => _profile.ToggleReactionAsync(jwtToken, postId);
        public Task<CommentDto?> AddCommentAsync(string jwtToken, int postId, string content) => _profile.AddCommentAsync(jwtToken, postId, content);

        // Assets
        public Task<List<ItAssetDto>> GetItAssetsAsync(int? roomId = null, int? campusId = null, string? searchTerm = null) => _assets.GetItAssetsAsync(roomId, campusId, searchTerm);
        public Task<(bool Success, string Message)> AddItAssetAsync(ItAssetDto asset) => _assets.AddItAssetAsync(asset);
        public Task<(bool Success, string Message)> UpdateItAssetAsync(ItAssetDto asset) => _assets.UpdateItAssetAsync(asset);
        public Task<(bool Success, string Message)> DeleteItAssetAsync(int id) => _assets.DeleteItAssetAsync(id);
        public Task<(bool Success, string Message)> TransferItAssetAsync(int assetId, int targetRoomId) => _assets.TransferItAssetAsync(assetId, targetRoomId);

        // Rooms
        public Task<List<RoomDto>> GetRoomsAsync(int? campusId = null) => _rooms.GetRoomsAsync(campusId);
        public Task<(bool Success, string Message)> AddRoomAsync(RoomDto room) => _rooms.AddRoomAsync(room);
        public Task<(bool Success, string Message)> UpdateRoomAsync(RoomDto room) => _rooms.UpdateRoomAsync(room);
        public Task<(bool Success, string Message)> DeleteRoomAsync(int roomId) => _rooms.DeleteRoomAsync(roomId);
        public Task<(bool Success, string Message)> ToggleRoomStatusAsync(int roomId) => _rooms.ToggleRoomStatusAsync(roomId);

        // Campuses
        public Task<List<CampusDto>> GetCampusesAsync(string? token = null) => _campuses.GetCampusesAsync(token);
        public Task<(bool Success, string Message)> AddCampusAsync(CampusDto campus) => _campuses.AddCampusAsync(campus);
        public Task<(bool Success, string Message)> UpdateCampusAsync(CampusDto campus) => _campuses.UpdateCampusAsync(campus);
        public Task<(bool Success, string Message)> DeleteCampusAsync(int id) => _campuses.DeleteCampusAsync(id);

        // User Management
        public Task<List<UserManagementDto>> GetUsersForManagementAsync() => _userMgmt.GetUsersForManagementAsync();
        public Task<(bool Success, string Message)> AddUserCampusAccessAsync(int userId, int campusId) => _userMgmt.AddUserCampusAccessAsync(userId, campusId);
        public Task<(bool Success, string Message)> RemoveUserCampusAccessAsync(int userId, int campusId) => _userMgmt.RemoveUserCampusAccessAsync(userId, campusId);
        public Task<(bool Success, string Message)> ToggleUserCampusBlockAsync(int userId, int campusId) => _userMgmt.ToggleUserCampusBlockAsync(userId, campusId);
        public Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto user) => _userMgmt.CreateUserAsync(user);
        public Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto user) => _userMgmt.UpdateUserAsync(userId, user);
        public Task<(bool Success, string Message)> DeleteUserAsync(int userId) => _userMgmt.DeleteUserAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId) => _userMgmt.ToggleUserStatusAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserITAccessAsync(int userId) => _userMgmt.ToggleUserITAccessAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserAPAccessAsync(int userId) => _userMgmt.ToggleUserAPAccessAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserMessageAccessAsync(int userId) => _userMgmt.ToggleUserMessageAccessAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserLibraryAccessAsync(int userId) => _userMgmt.ToggleUserLibraryAccessAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserHomeEconomicsAccessAsync(int userId) => _userMgmt.ToggleUserHomeEconomicsAccessAsync(userId);
        public Task<(bool Success, string Message)> ToggleUserConsumablesAccessAsync(int userId) => _userMgmt.ToggleUserConsumablesAccessAsync(userId);

        // Reports / Dashboard
        public Task<(DashboardDto? Stats, string Message)> GetDashboardStatsAsync(int campusId) => _reports.GetDashboardStatsAsync(campusId);
        public Task<List<AuditLogDto>> GetAuditLogsAsync(int? campusId = null, string? entityType = null) => _reports.GetAuditLogsAsync(campusId, entityType);
        public Task<PaginatedAuditLogsDto> GetAuditLogsPaginatedAsync(int? campusId, int page, int pageSize, string? entityType = null) => _reports.GetAuditLogsPaginatedAsync(campusId, page, pageSize, entityType);

        // Apparel
        public Task<PaginatedApparelDto> GetApparelAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5) => _apparel.GetApparelAsync(campusId, searchTerm, page, pageSize);
        public Task<PaginatedApparelItemDto> GetSoldItemsAsync(int? campusId = null, int page = 1, int pageSize = 10) => _apparel.GetSoldItemsAsync(campusId, page, pageSize);
        public Task<(bool Success, string Message)> AddApparelAsync(ApparelDto apparel) => _apparel.AddApparelAsync(apparel);
        public Task<(bool Success, string Message)> UpdateApparelAsync(ApparelDto apparel) => _apparel.UpdateApparelAsync(apparel);
        public Task<(bool Success, string Message)> DeleteApparelAsync(int id) => _apparel.DeleteApparelAsync(id);
        public Task<List<ApparelItemDto>> GetApparelItemsAsync(int apparelId) => _apparel.GetApparelItemsAsync(apparelId);
        public Task<(bool Success, string Message)> UpdateApparelItemStatusAsync(int itemId, string status) => _apparel.UpdateApparelItemStatusAsync(itemId, status);
        public Task<(bool Success, string Message)> AddApparelStockAsync(int apparelId, int quantity) => _apparel.AddApparelStockAsync(apparelId, quantity);
        public Task<List<ApparelItemDto>> QueryApparelItemsAsync(string? status, DateTime? startDate, DateTime? endDate, int? campusId) => _apparel.QueryApparelItemsAsync(status, startDate, endDate, campusId);

        // Library
        public Task<List<LibraryBookDto>> GetLibraryBooksAsync(int? campusId = null) => _library.GetLibraryBooksAsync(campusId);
        public Task<(bool Success, string Message)> AddLibraryBookAsync(LibraryBookDto book) => _library.AddLibraryBookAsync(book);
        public Task<(bool Success, string Message)> UpdateLibraryBookAsync(LibraryBookDto book) => _library.UpdateLibraryBookAsync(book);
        public Task<(bool Success, string Message)> AddLibraryBookItemAsync(int bookId, LibraryBookItemDto item) => _library.AddLibraryBookItemAsync(bookId, item);
        public Task<(bool Success, string Message)> UpdateLibraryBookItemAsync(int itemId, LibraryBookItemDto item) => _library.UpdateLibraryBookItemAsync(itemId, item);
        public Task<(bool Success, string Message)> DeleteLibraryBookItemAsync(int itemId) => _library.DeleteLibraryBookItemAsync(itemId);

        // Furniture
        public Task<List<FurnitureDto>> GetFurnituresAsync(int? roomId = null, int? campusId = null, string? searchTerm = null) => _furniture.GetFurnituresAsync(roomId, campusId, searchTerm);
        public Task<(bool Success, string Message)> AddFurnitureAsync(FurnitureDto furniture) => _furniture.AddFurnitureAsync(furniture);
        public Task<(bool Success, string Message)> UpdateFurnitureAsync(FurnitureDto furniture) => _furniture.UpdateFurnitureAsync(furniture);
        public Task<(bool Success, string Message)> TransferFurnitureAsync(int id, int targetRoomId) => _furniture.TransferFurnitureAsync(id, targetRoomId);

        // Consumables
        public Task<ConsumableListResponse> GetConsumablesAsync(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10) => _consumables.GetConsumablesAsync(campusId, searchTerm, page, pageSize);
        public Task<List<ConsumableItemDto>> GetConsumableItemsAsync(int consumableId) => _consumables.GetConsumableItemsAsync(consumableId);
        public Task<(bool Success, string Message)> CreateConsumableAsync(ConsumableDto consumable) => _consumables.CreateConsumableAsync(consumable);
        public Task<bool> UpdateConsumableItemStatusAsync(int itemId, string status) => _consumables.UpdateItemStatusAsync(itemId, status);
    }
}
