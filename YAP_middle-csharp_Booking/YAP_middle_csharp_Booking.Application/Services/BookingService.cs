using Microsoft.Extensions.Logging;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Domain.Exceptions;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Application.Services
{
    /// <summary>
    /// Сервис для работы с бронями
    /// </summary>
    public class BookingService(IBookingRepository repository, ILogger<BookingService> logger) : IBookingService
    {
        private readonly ILogger<BookingService> _logger = logger;
        private readonly IBookingRepository _repository = repository;

        /// <summary>
        /// Метод получения необработанных заявок
        /// </summary>
        /// <returns>Возвращает список</returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBookingAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[BookingService] [FindPendingBooking] Запрос на получение необработанных заявок");
            return await _repository.FindPendingBookingsAsync(cancellationToken);
        }

        /// <summary>
        /// Метод получения брони по УИ с проверкой прав доступа
        /// </summary>
        /// <param name="id">Уникальный идентификатор бронирования</param>
        /// <param name="currentUserId">Уникальный идентификатор пользователя запроса</param>
        /// <returns>Возвращает найденную бронь или 400</returns>
        public async Task<BookingModel?> FindByIdForUserAsync(Guid id, Guid idUserFromRequest, UserRoleEnum userRole, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("[BookingService] [FindByIdForUserAsync] Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindByIdAsync(id, cancellationToken);
            if (findBooking == null)
            {
                _logger.LogWarning("[BookingService] [FindByIdForUserAsync] Бронирование с id: {idBooking}, не найдено", id);
                throw new NotFoundExceptionApp("Бронирование не найдено!");
            }

            if (userRole != UserRoleEnum.Admin && findBooking.UserId != idUserFromRequest)
            {
                _logger.LogWarning("[BookingService] [FindByIdForUserAsync] Пользователь: {idUserFromRequest} пытается получить чужое бронирование", idUserFromRequest);
                throw new NotFoundExceptionApp("Ошибка получения бронирования");
            }

            return findBooking;
        }

        /// <summary>
        /// Метод для создания новой брони для события
        /// </summary>
        /// <param name="eventId">УИ события</param>
        /// <returns>Возвращает созданную бронь</returns>

        public async Task<BookingModel> CreateBookingAsync(Guid eventId, Guid userId, int seatsCount = 1, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[BookingService] [CreateBookingAsync] Попытка создать бронь для события {EventId}", eventId);

            if (seatsCount <= 0)
            {
                _logger.LogWarning("[BookingService] [CreateBookingAsync] Пользователь {UserId} создал запись на 0 мест", userId);
                throw new ValidationExceptionApp("Количество запрашиваемых мест должно быть больше 0");
            }

            int activeBookingsCount = await _repository.CheckActiveCountBookingByUserId(userId, cancellationToken);
            if (activeBookingsCount >= 10)
            {
                _logger.LogWarning("[BookingService] [CreateBookingAsync] Пользователь {UserId} превысил лимит активных броней", userId);
                throw new BookingLimitExceededException(10);
            }

            var newBooking = new BookingModel(eventId, userId, seatsCount);
            await _repository.CreateAsync(newBooking, cancellationToken);

            _logger.LogInformation("[BookingService] [CreateBookingAsync] Бронь создана: {Id}", newBooking.Id);
            return newBooking;
        }

        /// <summary>
        /// Обновление данных существующей записи
        /// </summary>
        /// <param name="entity">Объект сущности с обновленными данными</param>
        /// <returns>Возвращает обновленный объект сущности</returns>
        public async Task<BookingModel> UpdateAsync(BookingModel entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            var findBooking = await _repository.FindByIdAsync(entity.Id, cancellationToken);
            if (findBooking is null)
                throw new NotFoundExceptionApp("Booking не найден!");

            findBooking.Status = entity.Status;
            findBooking.ProcessedAt = entity.ProcessedAt;

            await _repository.UpdateAsync(findBooking, cancellationToken);
            return findBooking;
        }

        /// <summary>
        /// Метод отмены бронирования
        /// </summary>
        /// <param name="eventId">Принимает УИ события</param>
        /// <param name="bookingId">Принимает уникальный идентификатор бронирования</param>
        public async Task CancelledBookingAsync(Guid eventId, Guid bookingId, Guid currentUserId, UserRoleEnum currentUserRole, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("[BookingService] [CancelledBookingAsync] Попытка отмены бронирования: {bookingId}", bookingId);

            var findBooking = await _repository.FindByIdAsync(bookingId, cancellationToken);
            if (findBooking == null)
                throw new NotFoundExceptionApp("Бронирование не найдено");

            if (findBooking.UserId != currentUserId && currentUserRole != UserRoleEnum.Admin)
                throw new UnauthorizedOperationException();

            if (eventId != Guid.Empty && findBooking.EventId != eventId)
                throw new ValidationExceptionApp("Указанная бронь не принадлежит данному событию");

            if (findBooking.Status == BookingStatusEnum.Confirmed ||
                findBooking.Status == BookingStatusEnum.Rejected ||
                findBooking.Status == BookingStatusEnum.Cancelled)
            {
                throw new ValidationExceptionApp("Бронирование нельзя отменить, потому что оно уже обработано");
            }

            findBooking.Cancel();
            await _repository.UpdateAsync(findBooking, cancellationToken);

            _logger.LogInformation("[BookingService] [CancelledBookingAsync] Бронь: {bookingId} успешно отменена", bookingId);
        }

        /// <summary>
        /// Удаление записи из системы
        /// </summary>
        /// <param name="entity">Объект сущности для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        public async Task DeleteAsync(BookingModel entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            var findBooking = await _repository.FindByIdAsync(entity.Id, cancellationToken);
            if (findBooking is null)
                throw new NotFoundExceptionApp("Booking не найден!");

            await _repository.DeleteAsync(findBooking, cancellationToken);
        }
    }
}