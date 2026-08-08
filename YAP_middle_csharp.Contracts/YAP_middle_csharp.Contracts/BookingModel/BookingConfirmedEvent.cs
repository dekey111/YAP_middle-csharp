using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Contracts.BookingModel
{
    public record BookingConfirmedEvent( Guid BookingId, Guid EventId, Guid UserId, int SeatsCount, DateTime ConfirmedAt );
}
