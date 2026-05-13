using System;
using System.Collections.Generic;

namespace QuickStockApp.Models
{
    public class LibraryBookDto
    {
        public int ItemId { get; set; }
        public string? BookNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Author { get; set; }
        public string? CoAuthor { get; set; }
        public string? Class { get; set; }
        public string? Publisher { get; set; }
        public int? Year { get; set; }
        public string? Edition { get; set; }
        public string? Volumes { get; set; }
        public int? Pages { get; set; }
        public string? ISBN { get; set; }
        public DateTime? DateReceived { get; set; }
        public decimal? CostPrice { get; set; }
        public string? SourceOfFund { get; set; }
        public string? Donor { get; set; }
        public string? AcquisitionType { get; set; }
        public string? Remarks { get; set; }
        public string? Genre { get; set; }
        public string? Language { get; set; }
        public string? ShelfLocation { get; set; }
        public string? CallNumber { get; set; }
        public string? CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CampusId { get; set; }

        public List<LibraryBookItemDto> Items { get; set; } = new();
        
        // Helper properties for display
        public int Quantity => Items?.Count ?? 0;
        public int AvailableQuantity => Items?.Count(i => i.Status == "Available") ?? 0;
        public string Status => Quantity == 0 ? "No Copies" : (AvailableQuantity > 0 ? "Available" : "Checked Out");
        public string AccessionNumber => Items?.FirstOrDefault()?.AccessionNumber ?? "N/A";
        public string QRCode => Items?.FirstOrDefault()?.QRCode ?? "N/A";

        // For creation
        public string? InitialAccessionNumber { get; set; }
    }
}
