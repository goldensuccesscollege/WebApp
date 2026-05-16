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
        public string Unit { get; set; } = "Pieces";
        public int CampusId { get; set; }
    }

    public class ConsumableItemDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DateOut { get; set; }
        public int ConsumableDataId { get; set; }
        public string? AddedByUsername { get; set; }

        // Request Info
        public bool IsRequestPending { get; set; }
        public int? PendingRequestId { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? RequestedByUsername { get; set; }
        public DateTime? RequestedAt { get; set; }

        public string? ApprovedByUsername { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class ConsumableOutRequestDto
    {
        public int Id { get; set; }
        public int ConsumableItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int ConsumableDataId { get; set; }
        public string RequestedByUsername { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ApprovedByUsername { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }
        public int CampusId { get; set; }
    }
}
