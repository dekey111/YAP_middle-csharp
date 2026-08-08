using Microsoft.Extensions.Logging;
using YAP_middle_csharp_Booking.Application.Interfaces.IApi;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Domain.Exceptions;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Application.Services
{
    /// <summary>
    /// Сервис для работы с бронями
    /// </summary>
    public class BookingService(
        IBookingRepository repository,
        IEventApiClient eventApiClient,
        ILogger<BookingService> logger) : IBookingService
    {
        private readonly ILogger<BookingService> _logger = logger;
        private readonly IBookingRepository _repository = repository;
        private readonly IEventApiClient _eventApiClient = eventApiClient;

        private static readonly SemaphoreSlim _bookingSemaphore = new(1, 1);
        private static readonly SemaphoreSlim _bookingCancelledSemaphore = new(1, 1);


        /// <summary>
        /// Метод получения необработанных заявок
        /// </summary>
        /// <returns>Возвращает список</returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBookingAsync()
        {
            _logger.LogInformation("[BookingService] [FindPendingBooking] Запрос на получение необработанных заявок");
            return await _repository.FindPendingBookingsAsync();
        }

        /// <summary>
        /// Метод получения брони по УИ с проверкой прав доступа
        /// </summary>
        /// <param name="id">Уникальный идентификатор бронирования</param>
        /// <param name="currentUserId">Уникальный идентификатор пользователя запроса</param>
        /// <returns>Возвращает найденную бронь или 400</returns>
        public async Task<BookingModel?> FindByIdForUserAsync(Guid id, Guid idUserFromRequest, UserRoleEnum userRole)
        {
            _logger.LogDebug("[BookingService] [FindByIdForUserAsync] Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindByIdAsync(id);
            if(findBooking == null)
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

        public async Task<BookingModel> CreateBookingAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("[BookingService] [CreateBookingAsync] Попытка создать бронь для события {EventId}", eventId);

            await _bookingSemaphore.WaitAsync();
            try
            {
                var eventDto = await _eventApiClient.GetEventByIdAsync(eventId);
                if (eventDto == null)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Событие не найдено {EventId}", eventId);
                    throw new NotFoundExceptionApp("Событие не найдено");
                }

                if (DateTime.UtcNow >= eventDto.StartAt)
                    throw new ValidationExceptionApp("Нельзя забронировать событие, которое уже началось");

                if (DateTime.UtcNow >= eventDto.EndAt)
                    throw new ValidationExceptionApp("Срок регистрации на событие истек");

                int activeBookingsCount = await _repository.CheckActiveCountBookingByUserId(userId);
                if (activeBookingsCount >= 10)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Пользователь {UserId} превысил лимит активных броней", userId);
                    throw new BookingLimitExceededException(10);
                }

                var newBooking = new BookingModel(eventId, userId);
                await _repository.CreateAsync(newBooking);

                _logger.LogInformation("[BookingService] [CreateBookingAsync] Бронь создана: {Id}", newBooking.Id);
                return newBooking;
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }

        /// <summary>
        /// Обновление данных существующей записи
        /// </summary>
        /// <param name="entity">Объект сущности с обновленными данными</param>
        /// <returns>Возвращает обновленный объект сущности</returns>
        public async Task<BookingModel> UpdateAsync(BookingModel entity)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            var findBooking = await _repository.FindByIdAsync(entity.Id);
            if (findBooking is null)
                throw new NotFoundExceptionApp("Booking не найден!");

            findBooking.Status = entity.Status;
            findBooking.ProcessedAt = entity.ProcessedAt;

            await _repository.UpdateAsync(findBooking);
            return findBooking;
        }

        /// <summary>
        /// Метод отмены бронирования
        /// </summary>
        /// <param name="eventId">Принимает УИ события</param>
        /// <param name="bookingId">Принимает уникальный идентификатор бронирования</param>
        public async Task CancelledBookingAsync(Guid eventId, Guid bookingId, Guid currentUserId, UserRoleEnum currentUserRole)
        {
            _logger.LogWarning("[BookingService] [CancelledBookingAsync] Попытка отмены бронирования: {bookingId}", bookingId);

            await _bookingCancelledSemaphore.WaitAsync();
            try
            {
                var findBooking = await _repository.FindByIdAsync(bookingId);
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

                var eventDto = await _eventApiClient.GetEventByIdAsync(findBooking.EventId);
                if (eventDto != null && DateTime.UtcNow >= eventDto.StartAt)
                {
                    throw new ValidationExceptionApp("Нельзя отменить бронирование после начала или завершения события");
                }

                findBooking.Cancel();
                await _repository.UpdateAsync(findBooking);

                _logger.LogInformation("[BookingService] [CancelledBookingAsync] Бронь: {bookingId} успешно отменена", bookingId);
            }
            finally
            {
                _bookingCancelledSemaphore.Release();
            }
        }

        /// <summary>
        /// Удаление записи из системы
        /// </summary>
        /// <param name="entity">Объект сущности для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        public async Task DeleteAsync(BookingModel entity)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            var findBooking = await _repository.FindByIdAsync(entity.Id);
            if (findBooking is null)
                throw new NotFoundExceptionApp("Booking не найден!");

            await _repository.DeleteAsync(findBooking);
        }
    }
}