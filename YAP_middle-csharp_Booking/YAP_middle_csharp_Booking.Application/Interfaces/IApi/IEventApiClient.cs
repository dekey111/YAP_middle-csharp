
using YAP_middle_csharp.Contracts.EventModels;

namespace YAP_middle_csharp_Booking.Application.Interfaces.IApi
{
    public interface IEventApiClient
    {

        /// <summary>
        /// Получить информацию о событии по его ID
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Возвращает сущность события, иначе null</returns>
        Task<EventContract?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
