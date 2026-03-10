using System;

namespace QuickStockApp.Models
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string AuthorProfileImage { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
