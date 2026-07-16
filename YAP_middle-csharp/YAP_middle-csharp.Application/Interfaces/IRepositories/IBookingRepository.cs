using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;
    
namespace YAP_middle_csharp.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Интерфейс для работы с БД броней
    /// </summary>
    public interface IBookingRepository : IGenericRepository<BookingModel>
    {
        Task<IEnumerable<BookingModel>> FindPendingBookingsAsync();
        Task<PaginatedResult<BookingModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize);

    }
}
