namespace QuickStockApp.Models
{
    public class CampusDto
    {
        public int CampusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}
