namespace QuickStockApp.Models
{
    public class UserManagementDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public string Status { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool CanAccessITAssets { get; set; }
        public bool CanAccessApparel { get; set; }
        public bool CanAccessMessages { get; set; }
        public bool CanAccessLibrary { get; set; }
        public bool CanAccessHomeEconomics { get; set; }
        public bool CanAccessConsumables { get; set; }
        public List<UserCampusAccessDto> Campuses { get; set; } = new();
    }

    public class UserCampusAccessDto
    {
        public int CampusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool CanAccessITAssets { get; set; } = true;
        public bool CanAccessApparel { get; set; } = true;
        public bool CanAccessMessages { get; set; } = true;
        public bool CanAccessLibrary { get; set; } = true;
        public bool CanAccessHomeEconomics { get; set; } = true;
        public bool CanAccessConsumables { get; set; } = true;
    }

    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool CanAccessITAssets { get; set; }
        public bool CanAccessApparel { get; set; }
        public bool CanAccessMessages { get; set; }
        public bool CanAccessLibrary { get; set; }
        public bool CanAccessHomeEconomics { get; set; }
        public bool CanAccessConsumables { get; set; }
    }
}
