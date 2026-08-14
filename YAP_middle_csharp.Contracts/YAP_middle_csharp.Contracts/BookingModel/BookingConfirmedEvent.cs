using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace YAP_middle_csharp.Contracts.BookingModel
{
    public class BookingConfirmedEvent
    {
        public Guid BookingId { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int SeatsCount { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public BookingConfirmedEvent() { }

        public BookingConfirmedEvent(Guid bookingId, Guid eventId, Guid userId, int seatsCount, DateTime confirmedAt)
        {
            BookingId = bookingId;
            EventId = eventId;
            UserId = userId;
            SeatsCount = seatsCount;
            ConfirmedAt = confirmedAt;
        }
    }
}
