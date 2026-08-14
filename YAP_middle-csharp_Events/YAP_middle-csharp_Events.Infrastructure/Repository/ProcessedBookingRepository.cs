using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp_Events.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Events.Domain.Models;
using YAP_middle_csharp_Events.Infrastructure.DataAccess;

namespace YAP_middle_csharp_Events.Infrastructure.Repository
{
    public class ProcessedBookingRepository(AppDbContext dbContext) : IProcessedBookingRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProcessedBookings.AnyAsync(p => p.BookingId == bookingId, cancellationToken);
        }

        public async Task AddAsync(ProcessedBookingsModel processedBooking, CancellationToken cancellationToken = default)
        {
            await _dbContext.ProcessedBookings.AddAsync(processedBooking, cancellationToken);
        }
    }
}
