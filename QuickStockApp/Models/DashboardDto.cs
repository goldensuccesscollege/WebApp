namespace QuickStockApp.Models
{
    public class DashboardDto
    {
        // IT Assets
        public int TotalAssets { get; set; }
        public int RoomCount { get; set; }
        public int DisabledRooms { get; set; }
        public List<StatusCountDto> AssetsByStatus { get; set; } = new();
        public List<TypeCountDto> AssetsByType { get; set; } = new();
        public List<RecentActivityDto> RecentItActivities { get; set; } = new();

        // Apparel
        public int TotalApparelTypes { get; set; }
        public int TotalApparelInStock { get; set; }
        public int TotalApparelSold { get; set; }
        public List<CategoryCountDto> ApparelByCategory { get; set; } = new();
        public List<RecentActivityDto> RecentApparelActivities { get; set; } = new();

        // Furniture
        public int TotalFurniture { get; set; }
        public List<LocationCountDto> FurnitureByLocation { get; set; } = new();
        public List<ConditionCountDto> FurnitureByCondition { get; set; } = new();
        public List<RecentActivityDto> RecentFurnitureActivities { get; set; } = new();

        // Consumables
        public int TotalConsumableTypes { get; set; }
        public int TotalConsumableBalance { get; set; }
        public List<RecentActivityDto> RecentConsumableActivities { get; set; } = new();

        // Combined
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        // Enhanced Dashboard Properties
        public int ActiveAssets { get; set; }
        public int CriticalAssets { get; set; }
        public int AssetHealthPercentage { get; set; }
        public string? MostCommonAssetType { get; set; }
        public string? MostProblematicRoom { get; set; }
        public int MonthlyActivities { get; set; }
        public int NewAssetsThisMonth { get; set; }
        public string? SystemOverview { get; set; }
        public List<string> Alerts { get; set; } = new();
        public List<RoomAssetDto> AssetsByRoom { get; set; } = new();
    }

    public class RoomAssetDto
    {
        public string Room { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class LocationCountDto
    {
        public string Location { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ConditionCountDto
    {
        public string Condition { get; set; } = string.Empty;
        public int Count { get; set; }
    }


    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TypeCountDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CategoryCountDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
