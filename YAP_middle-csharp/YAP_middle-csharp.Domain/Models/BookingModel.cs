using System.Diagnostics.CodeAnalysis;

namespace YAP_middle_csharp.Models
{
    /// <summary>
    /// Базовая модель бронирований из БД
    /// </summary>
    public class BookingModel
    {
        public required Guid Id { get; set; }
        public required Guid EventId { get; set; }
        public required BookingStatusEnum Status { get; set; }

        private DateTimeOffset _createdAt; 
        public required DateTime CreatedAt 
        {
            get => _createdAt.UtcDateTime;
            set => _createdAt = value.Kind == DateTimeKind.Unspecified
                                           ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                                           : value.ToUniversalTime();
        }

        private DateTimeOffset? _processedAt;
        public DateTime? ProcessedAt
        {
            get => _processedAt?.UtcDateTime;
            set => _processedAt = value.HasValue 
                ? (value.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) 
                    : value.Value.ToUniversalTime())
                : null;
        }

        public EventModel Event { get; set; } = null!;



        [SetsRequiredMembers]
        private BookingModel()
        {
        }

        [SetsRequiredMembers]
        public BookingModel(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatusEnum.Pending;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
