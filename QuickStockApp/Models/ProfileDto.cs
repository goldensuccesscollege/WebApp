namespace QuickStockApp.Models
{
    public class ProfileDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageProfilePath { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        // Image is handled via MultipartFormDataContent
    }
}
