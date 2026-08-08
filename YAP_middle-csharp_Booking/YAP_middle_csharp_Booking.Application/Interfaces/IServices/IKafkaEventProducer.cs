using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Contracts.BookingModel;

namespace YAP_middle_csharp_Booking.Application.Interfaces.IServices
{
    public interface IKafkaEventProducer
    {
        Task ProduceBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmedEvent, CancellationToken cancellationToken = default);
    }
}
