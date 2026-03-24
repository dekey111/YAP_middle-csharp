namespace YAP_middle_csharp.Models
{
    public class EventModel
    {
        public required int id { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required DateTime StartAt { get; set; }
        public required DateTime EndAt { get; set; }
    }
}
