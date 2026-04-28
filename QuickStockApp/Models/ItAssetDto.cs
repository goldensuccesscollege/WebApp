namespace QuickStockApp.Models
{
    public class ItAssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Qrcode { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? RoomId { get; set; }
        public int CampusId { get; set; }
    }
}
