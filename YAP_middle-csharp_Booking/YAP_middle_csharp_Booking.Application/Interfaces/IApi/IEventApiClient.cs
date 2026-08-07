
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

        /// <summary>
        /// Попытка зарезервировать место на событие
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="seatsCount">Количество мест (по умолчанию 1) </param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true - при успехе, иначе false </returns>
        Task<bool> TryReserveSeatAsync(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default);


        /// <summary>
        /// Освободить место на событие (при отмене брони)
        /// </summary>
        /// <param name="eventId">Уникальный идентификатор события</param>
        /// <param name="seatsCount">Количество мест (по умолчанию 1) </param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true - при успехе, иначе false </returns>
        Task<bool> ReleaseSeatAsync(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default);
    }
}
