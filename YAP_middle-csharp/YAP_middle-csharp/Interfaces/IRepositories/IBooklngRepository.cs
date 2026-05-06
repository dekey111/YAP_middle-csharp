using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces.IRepositories
{
    public interface IBooklngRepository : IRepository<BookingModel>
    {
        Task<IEnumerable<BookingModel>> FindPendingBookings();
    }
}
