using System;
using System.Text.Json.Serialization;

namespace QuickStockApp.Models
{
    public class FurnitureDto
    {
        [JsonPropertyName("item_ID")]
        public int Item_ID { get; set; }

        [JsonPropertyName("item_Name")]
        public string Item_Name { get; set; } = string.Empty;

        [JsonPropertyName("item_number")]
        public string? Item_number { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("dateAdded")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("qrcode")]
        public string? Qrcode { get; set; }

        [JsonPropertyName("item_count")]
        public int Item_count { get; set; } = 1;

        [JsonPropertyName("roomId")]
        public int? RoomId { get; set; }

        [JsonPropertyName("campusId")]
        public int CampusId { get; set; }
        
        // Navigation properties (simplified for DTO)
        public string? RoomName { get; set; }
        public string? CampusName { get; set; }
        public int TotalItemsInRoom { get; set; }
    }
}
