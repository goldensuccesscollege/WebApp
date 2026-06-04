namespace QuickStockApp.Models
{
    public class ConsumableResponse
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public int Count { get; set; }
        public DateTime? DateArrive { get; set; }
        public int CampusId { get; set; }
    }

    public class CreateConsumableCommand
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int CampusId { get; set; }
    }

    public class AddStockCommand
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class DeductStockCommand
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class ConsumableRequestDto
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int? TargetItemId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? RequestorId { get; set; }
        public string? RequestorName { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public int CampusId { get; set; }
    }

    public class CreateConsumableRequestCommand
    {
        public string RequestType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int? TargetItemId { get; set; }
        public int CampusId { get; set; }
        public string? RequestorId { get; set; }
        public string? RequestorName { get; set; }
        public string SubmitToken { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }

    public class RejectConsumableRequestCommand
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    // NEW: Data Transfer Object for Ledger Entries
    public class ConsumableLedgerEntryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public DateTime Date { get; set; }                  // Fixes 3 error CS1061 warnings
        public string Action { get; set; } = string.Empty;
        public int In { get; set; }
        public int Out { get; set; }
        public int Balance { get; set; }                   // Fixes 2 error CS1061 warnings
        public string ProcessedByName { get; set; } = string.Empty; // Fixes 4 error CS1061 warnings
        public int CampusId { get; set; }
    }
}
