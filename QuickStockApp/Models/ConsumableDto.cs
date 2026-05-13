using System;

namespace QuickStockApp.Models
{
    public class ConsumableDto
    {
        public int Id { get; set; }
        public string Product { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateArrived { get; set; }
        public int In { get; set; }
        public int Out { get; set; }
        public int Balance { get; set; }
        public int CampusId { get; set; }
    }

    public class ConsumableItemDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DateOut { get; set; }
        public int ConsumableDataId { get; set; }
    }
}
