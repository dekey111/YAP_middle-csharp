using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces.IServices
{
    public interface IBookingServive : IQueryService<BookingModel>, ICommandService<BookingModel>
    {
        Task<IEnumerable<BookingModel>> FindPendingBooking();

        Task<BookingModel> CreateBookingAsync(Guid eventId);
    }
}
