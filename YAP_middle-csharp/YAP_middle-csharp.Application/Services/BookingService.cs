using Microsoft.Extensions.Logging;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Services
{
    /// <summary>
    /// Сервис для работы с бронями
    /// </summary>
    /// <param name="repository">Принимает репозиторий брони</param>
    /// <param name="logger">Принимает логгер</param>
    public class BookingService(IBookingRepository repository,
        ILogger<BookingService> logger,
        IEventService eventService ) : IBookingService
    {
        private readonly ILogger<BookingService> _logger = logger;
        private readonly IBookingRepository _repository = repository;
        private readonly IEventService _eventService = eventService;

        private readonly static SemaphoreSlim _bookingSemaphore = new (1, 1);
        private readonly static SemaphoreSlim _bookingCancelledSemaphore = new (1, 1);


        /// <summary>
        /// Метод для поиска Бронирований с опциональными фильтрами 
        /// </summary>
        /// <param name="status">Фильтр по статусу</param>
        /// <param name="created">Фильтр по дате создания</param>
        /// <param name="processed">Фильтр по дате обработки</param>
        /// <param name="page">Сортировка по странице</param>
        /// <param name="pageSize">Сортировка по записям</param>
        /// <returns>Возвращается PaginatedResult<BookingModel> </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PaginatedResult<BookingModel>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
        {
            _logger.LogDebug("[BookingService] [FindAll] Начало выполнения FindAll: title={Title}, from={From}, to={To}, page={Page}, pSize={pSize}",
                title, from, to, page, pageSize);

            if (page < 1)
            {
                _logger.LogWarning("[BookingService] [FindAll] Передан некорректный номер страницы: {Page}", page);
                throw new ValidationExceptionApp("Номер страницы должен быть не менее 1");
            }

            if (pageSize < 1 || pageSize > 200)
            {
                _logger.LogWarning("[BookingService] [FindAll] Передан некорректный размер страницы: {PageSize}", pageSize);
                throw new ValidationExceptionApp("Размер страницы должен быть от 1 до 200");
            }

            var result = await _repository.GetPagedAsync(title, from, to, page, pageSize);


            _logger.LogInformation("[BookingService] [FindAll] Выполнен FindAll: title={Title}, from={From}, to={To}, page={Page}, pSize={pSize}. Получилось: {TotalCount}",
                title, from, to, page, pageSize, result.Items.Count());

            return result;
        }

        /// <summary>
        /// Метод для получения всех необработанных заявок 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBookingAsync()
        {
            _logger.LogInformation("[BookingService] [FindPendingBooking] Запрос на получения необработанных заявок");
            return await _repository.FindPendingBookingsAsync();
        }

        /// <summary>
        /// Метод получения конкретной брони по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор брони</param>
        /// <returns>Возвращает экземпляр BookingModel в случае нахождения в противном случае null </returns>
        public async Task<BookingModel?> FindByIdAsync(Guid id)
        {
            _logger.LogDebug("[BookingService] [FindById] Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindByIdAsync(id);
            if (findBooking == null)
            {
                _logger.LogWarning("[BookingService] [FindById] Бронь {BookingId} не найдена", id);
                throw new NotFoundExceptionApp($"Бронь не найдена");
            }
            _logger.LogInformation($"[BookingService] [FindById] Получилось найти Booking с ID = {id}", id);

            return findBooking;
        }

        /// <summary>
        /// Добавление новой брони
        /// </summary>
        /// <param name="entity">Принимает модель брони</param>
        /// <returns>Возвращает уникальный идентификатор новой брони</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если передана пустая модель</exception>
        public async Task<Guid> CreateAsync(BookingModel entity)
        {
            _logger.LogDebug("[BookingService] [Create] Попытка Create Booking. entity = {@entity} ", entity);
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Create] Ошибка Booking. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            await _repository.CreateAsync(entity);
            _logger.LogInformation("[BookingService] [Create] Создание нового Booking с Id: {Id} ", entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// Метод для создания новой брони для события
        /// </summary>
        /// <param name="eventId">Получает УИ События</param>
        /// <returns>Возвращает экземпляр созданного события</returns>
        /// <exception cref="NotFoundExceptionApp">В случае если не найден Event</exception>
        /// <exception cref="ValidationExceptionApp">В случае ошибки валидации</exception>
        /// <exception cref="NoAvailableSeatsExceptionApp">В случае если мест не хватает</exception>
        public async Task<BookingModel> CreateBookingAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("[BookingService] [CreateBookingAsync] Попытка создать бронь для события {EventId}", eventId);

            await _bookingSemaphore.WaitAsync();
            try
            {
                var findEvent = await _eventService.FindByIdAsync(eventId);
                if (findEvent == null)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Событие не найдено {EventId}", eventId);
                    throw new NotFoundExceptionApp("Событие не найдено");
                }

                if (DateTime.UtcNow >= findEvent.StartAt)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Попытка забронировать уже начавшееся событие {EventId}", eventId);
                    throw new ValidationExceptionApp("Нельзя забронировать событие, которое уже началось");
                }

                if (DateTime.UtcNow >= findEvent.EndAt)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Срок регистрации на событие истек {EventId}", eventId);
                    throw new ValidationExceptionApp("Срок регистрации на событие истек");
                }

                int activeBookingsCount = await _repository.CheckActiveCountBookingByUserId(userId);
                if (activeBookingsCount >= 10)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Пользователь {UserId} превысил лимит активных броней", userId);
                    throw new BookingLimitExceededException(10); 
                }

                bool hasSeat = findEvent.TryReserveSeats(1);
                if (!hasSeat)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Недостаточно мест на событие {EventId}", eventId);
                    throw new NoAvailableSeatsExceptionApp("Недостаточно мест на событие");
                }

                var newBooking = new BookingModel(eventId, userId);

                await _repository.CreateAsync(newBooking);

                _logger.LogInformation("[BookingService] [CreateBookingAsync] Бронь создана: {Id}", newBooking.Id);
                return newBooking;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[BookingService] [CreateBookingAsync] Произошла ошибка при бронировании");
                throw;
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }

        /// <summary>
        /// Метод изменения существующей брони
        /// </summary>
        /// <param name="entity">Принимает модель брони</param>
        /// <returns>Возвращает обновлённую модель брони</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такой брони по ID не найдено</exception>
        public async Task<BookingModel> UpdateAsync(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Update] Ошибка Update. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findBooking = await _repository.FindByIdAsync(entity.Id);
            if (findBooking is null)
            {
                _logger.LogError("[BookingService] [Update] Ошибка Update. Booking с ID: {id}, не найдено!", entity.Id);
                throw new NotFoundExceptionApp("Booking не найден!");
            }

            findBooking.Status = entity.Status;
            findBooking.ProcessedAt = entity.ProcessedAt;

            await _repository.UpdateAsync(findBooking);

            _logger.LogInformation("[BookingService] [Update] Booking обновлён. id={id}", entity.Id);
            return findBooking;
        }


        /// <summary>
        /// Метод удаления брони
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если тако по ID не найдено</exception>
        public async Task DeleteAsync(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Delete] Ошибка Delete. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findBooking = await _repository.FindByIdAsync(entity.Id);
            if (findBooking is null)
            {
                _logger.LogWarning("[BookingService] [Delete] Ошибка Delete. Booking с ID {Id} не найдено", entity.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            _logger.LogDebug("[BookingService] [Delete] Попытка Delete Booking. entity = {@entity} ", findBooking);

            await _repository.DeleteAsync(findBooking);

            _logger.LogInformation("[BookingService] [Delete] Booking удалён: id={Id}", findBooking.Id);
        }

        /// <summary>
        /// Метод отмены бронирования
        /// </summary>
        /// <param name="bookingId">Принимает УИ бронирования </param>
        public async Task CancelledBookingAsync(Guid eventId, Guid bookingId, Guid currentUserId, UserRoleEnum currentUserRole)
        {
            _logger.LogWarning("[BookingService] [CancelledBookingAsync] Попытка отмены бронирования: {bookingId} у события: {eventId}", bookingId, eventId);

            await _bookingCancelledSemaphore.WaitAsync();
            try
            {
                var findEvent = await _eventService.FindByIdAsync(eventId);
                if (findEvent == null)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Событие не найдено {EventId}", eventId);
                    throw new NotFoundExceptionApp("Событие не найдено");
                }

                var findBooking = await _repository.FindByIdAsync(bookingId);
                if (findBooking == null)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Бронь id: {bookingId} на событие: {eventId} не найдена! ", bookingId, eventId);
                    throw new NotFoundExceptionApp("Бронирование не найдено");
                }

                if (findBooking.UserId != currentUserId && currentUserRole != UserRoleEnum.Admin)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Пользователь: {UserId} пытается отменить чужую бронь: {BookingId}", currentUserId, bookingId);
                    throw new UnauthorizedOperationException(); 
                }

                if (findBooking.EventId != eventId)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Указанная бронь: {bookingId} не принадлежит данному событию: {eventId}", bookingId, eventId);
                    throw new ValidationExceptionApp("Указанная бронь не принадлежит данному событию");
                }

                if (findBooking.Status == BookingStatusEnum.Confirmed || findBooking.Status == BookingStatusEnum.Rejected || findBooking.Status == BookingStatusEnum.Cancelled)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Бронирование: {bookingId} нельзя отменить, потому что оно уже обработано", bookingId);
                    throw new ValidationExceptionApp("Бронирование нельзя отменить, потому что оно уже обработано");
                }

                if (DateTime.UtcNow >= findEvent.EndAt)
                {
                    _logger.LogWarning("[BookingService] [CancelledBookingAsync] Срок регистрации на событие истек {eventId}", eventId);
                    throw new ValidationExceptionApp("Бронирование нельзя отменить, потому что cрок регистрации на событие истек");
                }

                findBooking.Cancel();
                findEvent.ReleaseSeats();

                await _repository.UpdateAsync(findBooking);
                _logger.LogInformation("[BookingService] [CancelledBookingAsync] Бронь: {bookingId} для события: {eventId} успешно отменена", bookingId, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[BookingService] [CancelledBookingAsync] Произошла ошибка при отмене бронирования: {bookingId} для события: {eventId}", bookingId, eventId); 
                throw;
            }
            finally
            {
                _bookingCancelledSemaphore.Release();
            }
        }
    }
}
