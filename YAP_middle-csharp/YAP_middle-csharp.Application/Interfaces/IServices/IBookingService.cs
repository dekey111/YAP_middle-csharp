
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
        Task CancelledBookingAsync(Guid eventId, Guid bookingId, Guid currentUserId, UserRoleEnum currentUserRole);

        /// <summary>
        /// Метод получения брони по УИ с проверкой прав доступа
        /// </summary>
        /// <param name="id">Уникальный идентификатор бронирования</param>
        /// <param name="currentUserId">Уникальный идентификатор пользователя запроса</param>
        /// <returns>Возвращает найденную бронь или 400</returns>
        Task<BookingModel?> FindByIdForUserAsync(Guid id, Guid idUserFromRequest);
    }
}
