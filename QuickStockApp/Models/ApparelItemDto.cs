namespace QuickStockApp.Models
{
    public class ApparelItemDto
    {
        public int Id { get; set; }
        public int AppareldataId { get; set; }
        public string Apparel_Number { get; set; } = string.Empty;
        public string Status { get; set; } = "In Stock";
        public int CampusId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public ApparelDto? ApparelType { get; set; }
    }
}
