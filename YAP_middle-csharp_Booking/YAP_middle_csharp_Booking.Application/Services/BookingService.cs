using Microsoft.Extensions.Logging;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Application.Models;
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

        public async Task<PaginatedResult<BookingModel>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
        {
            _logger.LogDebug("[BookingService] [FindAll] Начало выполнения FindAll: page={Page}, pSize={pSize}", page, pageSize);

            if (page < 1)
                throw new ValidationExceptionApp("Номер страницы должен быть не менее 1");

            if (pageSize < 1 || pageSize > 200)
                throw new ValidationExceptionApp("Размер страницы должен быть от 1 до 200");

            return await _repository.GetPagedAsync(title, from, to, page, pageSize);
        }

        public async Task<IEnumerable<BookingModel>> FindPendingBookingAsync()
        {
            _logger.LogInformation("[BookingService] [FindPendingBooking] Запрос на получение необработанных заявок");
            return await _repository.FindPendingBookingsAsync();
        }

        public async Task<BookingModel> FindByIdAsync(Guid id)
        {
            _logger.LogDebug("[BookingService] [FindByIdAsync] Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindByIdAsync(id);
            if (findBooking == null)
            {
                _logger.LogWarning("[BookingService] [FindByIdAsync] Бронь {BookingId} не найдена", id);
                throw new NotFoundExceptionApp("Бронь не найдена");
            }

            return findBooking;
        }

        /// <summary>
        /// Поиск брони с проверкой прав (Без запроса в БД Users — роль берем из claims токена)
        /// </summary>
        public async Task<BookingModel?> FindByIdForUserAsync(Guid id, Guid idUserFromRequest, UserRoleEnum userRole)
        {
            _logger.LogDebug("[BookingService] [FindByIdForUserAsync] Попытка найти Booking с ID = {id}", id);

            var findBooking = await FindByIdAsync(id);

            if (userRole != UserRoleEnum.Admin && findBooking.UserId != idUserFromRequest)
            {
                _logger.LogWarning("[BookingService] [FindByIdForUserAsync] Пользователь: {idUserFromRequest} пытается получить чужое бронирование", idUserFromRequest);
                throw new NotFoundExceptionApp("Ошибка получения бронирования");
            }

            return findBooking;
        }

        public async Task<Guid> CreateAsync(BookingModel entity)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            await _repository.CreateAsync(entity);
            return entity.Id;
        }

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

                bool seatReserved = await _eventApiClient.TryReserveSeatAsync(eventId);
                if (!seatReserved)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Недостаточно мест на событие {EventId}", eventId);
                    throw new NoAvailableSeatsExceptionApp("Недостаточно мест на событие");
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

        public async Task DeleteAsync(BookingModel entity)
        {
            if (entity is null)
                throw new ValidationExceptionApp(nameof(entity));

            var findBooking = await _repository.FindByIdAsync(entity.Id);
            if (findBooking is null)
                throw new NotFoundExceptionApp("Booking не найден!");

            await _repository.DeleteAsync(findBooking);
        }

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

                await _eventApiClient.ReleaseSeatAsync(findBooking.EventId);

                _logger.LogInformation("[BookingService] [CancelledBookingAsync] Бронь: {bookingId} успешно отменена", bookingId);
            }
            finally
            {
                _bookingCancelledSemaphore.Release();
            }
        }

    }
