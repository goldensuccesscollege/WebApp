namespace QuickStockApp.Models
{
    public class DashboardDto
    {
        public int TotalAssets { get; set; }
        public int RoomCount { get; set; }
        public int DisabledRooms { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
