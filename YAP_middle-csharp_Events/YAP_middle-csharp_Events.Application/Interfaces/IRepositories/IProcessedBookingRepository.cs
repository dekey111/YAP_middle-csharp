using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Interfaces.IRepositories
{
    public interface IProcessedBookingRepository
    {
        Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken = default);
        Task AddAsync(ProcessedBookingsModel bookingId, CancellationToken cancellationToken = default);
    }
}
