
using YAP_middle_csharp.Domain.Models;

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
        Task<BookingModel> CreateBookingAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Метод отмены бронирования
        /// </summary>
        /// <param name="eventId">Принимает УИ события</param>
        /// <param name="bookingId">Принимает уникальный идентификатор бронирования</param>
        Task CancelledBookingAsync(Guid eventId, Guid bookingId);
    }
}
