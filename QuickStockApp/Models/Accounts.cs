namespace QuickStockApp.Models
{
    public class Accounts
    {
    }
    public class LoginRequestDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public System.Collections.Generic.List<int> CampusIds { get; set; } = new();
        public bool CanAccessITAssets { get; set; }
        public bool CanAccessApparel { get; set; }
        public bool CanAccessMessages { get; set; }
        public bool CanAccessLibrary { get; set; }
        public bool CanAccessHomeEconomics { get; set; }
        public bool CanAccessConsumables { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public bool IsFromApi { get; set; } = false;
    }
}
