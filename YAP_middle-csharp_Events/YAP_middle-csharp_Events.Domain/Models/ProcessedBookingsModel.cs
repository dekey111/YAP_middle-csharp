using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Domain.Models
{
    public class ProcessedBookingsModel
    {
        public Guid Id { get; private set; }
        public Guid BookingId { get; private set; }
        public DateTime ProcessedAt { get; private set;  }
        public ProcessedBookingsModel(Guid bookingId)
        {
            Id = Guid.NewGuid();
            BookingId = bookingId;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
