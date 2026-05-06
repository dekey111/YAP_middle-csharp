using System.Diagnostics.CodeAnalysis;

namespace YAP_middle_csharp.Models
{
    public class BookingModel
    {
        public required Guid Id { get; set; }
        public required Guid EventId { get; set; }
        public required BookingStatusEnum Status { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        [SetsRequiredMembers]
        public BookingModel()
        {
            Id = Guid.NewGuid();
            Status = BookingStatusEnum.Pending;
            CreatedAt = DateTime.Now;
        }
    }
}
