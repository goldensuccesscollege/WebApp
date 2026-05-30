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
        public DateTime Timestamp { get; set; }
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
    }

    public class RejectConsumableRequestCommand
    {
        public string RejectionReason { get; set; } = string.Empty;
    }
}
