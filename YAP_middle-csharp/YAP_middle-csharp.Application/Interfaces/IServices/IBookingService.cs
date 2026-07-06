using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Application.Interfaces.IServices
{
    public interface IBookingService : IQueryService<BookingModel>, ICommandService<BookingModel>
    {
        /// <summary>
        /// Метод получения необработанных заявок
        /// </summary>
        /// <returns>Возвращает список</returns>
        Task<IEnumerable<BookingModel>> FindPendingBookingAsync();

        /// <summary>
        /// Метод для создания новой брони для события
        /// </summary>
        /// <param name="eventId">УИ события</param>
        /// <returns>Возвращает созданную бронь</returns>
        Task<BookingModel> CreateBookingAsync(Guid eventId);
    }
}
