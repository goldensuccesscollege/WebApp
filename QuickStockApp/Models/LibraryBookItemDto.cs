using System;

namespace QuickStockApp.Models
{
    public class LibraryBookItemDto
    {
        public int Id { get; set; }
        public int LibrarydataId { get; set; }
        public string AccessionNumber { get; set; } = string.Empty;
        public string? QRCode { get; set; }
        public string Status { get; set; } = "Available";
        public string? Condition { get; set; }
        public int CampusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
