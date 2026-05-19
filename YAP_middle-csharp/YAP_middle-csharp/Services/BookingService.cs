using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace YAP_middle_csharp.Services
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
        private readonly object _bookingLock = new(); 


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
        public async Task<PaginatedResult<BookingModel>> FindAll(
            string? title = null,
            DateTime? from = null,
            DateTime? to = null, 
            int page = 1,
            int pageSize = 10)

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

            var query = await _repository.StartQueryToFindAll();

            if (!string.IsNullOrEmpty(title))
            {
                if (Enum.TryParse<BookingStatusEnum>(title, true, out var statusFilter))
                {
                    query = query.Where(x => x.Status == statusFilter);
                }
            }
            if (from.HasValue && from is not null)
            {
                query = query.Where(x => x.CreatedAt.Date == from.Value.Date);
            }
            if (to.HasValue && to is not null)
            {
                query = query.Where(x => x.ProcessedAt != null && x.ProcessedAt.Value.Date == to.Value.Date);
            }
            var totalCount = query.Count();
            var resultQuery = query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new PaginatedResult<BookingModel>
            {
                Items = resultQuery,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            _logger.LogInformation("[BookingService] [FindAll] Выполнен FindAll: title={Title}, from={From}, to={To}, page={Page}, pSize={pSize}. Получилось: {TotalCount}",
                title, from, to, page, pageSize, result.Items.Count());

            return result;
        }

        /// <summary>
        /// Метод для получения всех необработанных заявок 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBooking()
        {
            _logger.LogInformation("[BookingService] [FindPendingBooking] Запрос на получения необработанных заявок");
            return await _repository.FindPendingBookings();
        }

        /// <summary>
        /// Метод получения конкретной брони по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор брони</param>
        /// <returns>Возвращает экземпляр BookingModel в случае нахождения в противном случае null </returns>
        public async Task<BookingModel?> FindById(Guid id)
        {
            _logger.LogDebug("[BookingService] [FindById] Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindById(id);
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
        public async Task<Guid> Create(BookingModel entity)
        {
            _logger.LogDebug("[BookingService] [Create] Попытка Create Booking. entity = {@entity} ", entity);
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Create] Ошибка Booking. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            await _repository.Create(entity);
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
        public async Task<BookingModel> CreateBookingAsync(Guid eventId)
        {
            _logger.LogInformation("[BookingService] [CreateBookingAsync] Попытка создать бронь для события {EventId}", eventId);

            lock (_bookingLock)
            {
                var findEvent = _eventService.FindById(eventId).GetAwaiter().GetResult();
                if (findEvent == null)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Событие не найдено {EventId}", eventId);
                    throw new NotFoundExceptionApp("Событие не найдено");
                }

                if (DateTime.UtcNow >= findEvent.EndAt)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Срок регистрации на событие истек {EventId}", eventId);
                    throw new ValidationExceptionApp("Срок регистрации на событие истек");
                }
                bool hasSeat = findEvent.TryReserveSeats(1).Result; 
                if (!hasSeat)
                {
                    _logger.LogWarning("[BookingService] [CreateBookingAsync] Недостаточно мест на событие {EventId}", eventId);
                    throw new NoAvailableSeatsExceptionApp("Недостаточно мест на событие");
                }

                var newBooking = new BookingModel { EventId = eventId };
                _repository.Create(newBooking).GetAwaiter().GetResult();
                _logger.LogInformation("[BookingService] [CreateBookingAsync] Бронь создана: {Id}", newBooking.Id);
                return newBooking;
            }

        }

        /// <summary>
        /// Метод изменения существующей брони
        /// </summary>
        /// <param name="entity">Принимает модель брони</param>
        /// <returns>Возвращает обновлённую модель брони</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такой брони по ID не найдено</exception>
        public async Task<BookingModel> Update(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Update] Ошибка Update. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findBooking = await _repository.FindById(entity.Id);
            if (findBooking is null)
            {
                _logger.LogError("[BookingService] [Update] Ошибка Update. Booking с ID: {id}, не найдено!", entity.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            _logger.LogDebug("[BookingService] [Update] Попытка обновления Booking. entity = {@entity} ", findBooking);

            await _repository.Update(entity);

            _logger.LogInformation("[BookingService] [Update] Booking обновлён. Новые данные: entity = {@entity} ", entity);

            return entity;
        }

        /// <summary>
        /// Метод удаления брони
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если тако по ID не найдено</exception>
        public async Task Delete(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("[BookingService] [Delete] Ошибка Delete. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findBooking = await _repository.FindById(entity.Id);
            if (findBooking is null)
            {
                _logger.LogWarning("[BookingService] [Delete] Ошибка Delete. Booking с ID {Id} не найдено", entity.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            _logger.LogDebug("[BookingService] [Delete] Попытка Delete Booking. entity = {@entity} ", findBooking);

            await _repository.Delete(findBooking);

            _logger.LogInformation("[BookingService] [Delete] Booking удалён: id={Id}", findBooking.Id);
        }
    }
}
