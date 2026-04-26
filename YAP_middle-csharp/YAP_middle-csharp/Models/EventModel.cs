using System.ComponentModel.DataAnnotations;

namespace YAP_middle_csharp.Models
{
    public class EventModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required DateTime StartAt { get; set; }
        public required DateTime EndAt { get; set; }
    }
}
