using System;
using System.Text.Json.Serialization;

namespace zorgApp.Models
{
    public class DiaryItem
    {
        [JsonIgnore]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string PatientId { get; set; } = string.Empty;  // New field to link to patient
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public string CreatedBy { get; set; } = string.Empty;
    }
}
