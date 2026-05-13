namespace QuickStockApp.Models
{
    public class ApparelDto
    {
        public int Apparel_ID { get; set; }
        public string Apparel_Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Grade_Level { get; set; } = string.Empty;
        public int Quality_In_Stock { get; set; }
        public int Reorder_level { get; set; }
        public DateTime Date_Purchased { get; set; } = DateTime.Now;
        public decimal Unit_Price { get; set; }
        public string Supplier_Name { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? Location { get; set; }
        public int CampusId { get; set; }
    }
    public class PaginatedApparelDto
    {
        public int TotalItems { get; set; }
        public List<ApparelDto> Apparel { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedApparelItemDto
    {
        public int TotalItems { get; set; }
        public List<ApparelItemDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
