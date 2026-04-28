namespace QuickStockApp.Models
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomFloor { get; set; } = string.Empty;
        public string? RoomDescription { get; set; }
        public int CampusId { get; set; }
        public bool IsDisabled { get; set; }
    }
}
