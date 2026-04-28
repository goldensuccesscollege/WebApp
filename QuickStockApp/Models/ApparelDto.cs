namespace QuickStockApp.Models
{
    public class ApparelDto
    {
        public int Apparel_ID { get; set; }
        public string Apparel_Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Grade_Level { get; set; } = string.Empty;
        public int Quality_In_Stock { get; set; }
        public int Reorder_level { get; set; }
        public DateTime Date_Purchased { get; set; } = DateTime.Now;
        public decimal Unit_Price { get; set; }
        public string Supplier_Name { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int CampusId { get; set; }
    }
}
