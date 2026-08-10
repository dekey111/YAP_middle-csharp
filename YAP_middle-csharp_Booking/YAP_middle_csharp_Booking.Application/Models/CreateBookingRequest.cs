using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Booking.Application.Models
{
    public class CreateBookingRequest
    {
        public Guid EventId { get; set;  }
        public int SeatsCount { get; set; } = 1;
    }
}
