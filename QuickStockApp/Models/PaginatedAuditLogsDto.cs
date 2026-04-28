namespace QuickStockApp.Models
{
    public class PaginatedAuditLogsDto
    {
        public List<AuditLogDto> Logs { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
