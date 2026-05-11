using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces.IRepositories
{
    public interface IBookingRepository : IGenericRepository<BookingModel>
    {
        Task<IEnumerable<BookingModel>> FindPendingBookings();
    }
}
