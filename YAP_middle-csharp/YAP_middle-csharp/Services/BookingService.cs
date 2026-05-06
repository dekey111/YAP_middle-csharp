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
    public class BookingService(IBooklngRepository repository,
        ILogger<BookingService> logger,
        IEventService eventService ) : IBookingServive
    {
        private readonly ILogger<BookingService> _logger = logger;
        private readonly IBooklngRepository _repository = repository;
        private readonly IEventService _eventService = eventService;


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
            _logger.LogDebug("Начало выполнения FindAll: title={Title}, from={From}, to={To}, page ={Page}, pSize={pSize}",
                title, from, to, page, pageSize);

            if (page < 1)
            {
                _logger.LogWarning("Передан некорректный номер страницы: {Page}", page);
                throw new ArgumentException("Номер страницы должен быть не менее 1");
            }

            if (pageSize < 1 || pageSize > 200)
            {
                _logger.LogWarning("Передан некорректный размер страницы: {PageSize}", pageSize);
                throw new ArgumentException("Размер страницы должен быть от 1 до 200");
            }

            var findAllBooking = await _repository.FindAll();
            var query = findAllBooking.AsEnumerable();

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
                query = query.Where(x => x.ProcessedAt is not null && x.ProcessedAt.Value.Date == to.Value.Date);
            }
            var totalCount = query.Count();
            query = query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize);

            var result = new PaginatedResult<BookingModel>
            {
                Items = query,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            _logger.LogInformation("Выполнен FindAll: title={Title}, from={From}, to={To}, page ={Page}, pSize={pSize}. Получилось: {TotalCount}",
                title, from, to, page, pageSize, result.Items.Count());

            return result;
        }

        public async Task<IEnumerable<BookingModel>> FindPendingBooking()
        {
            _logger.LogInformation("Запрос на получения необрабоанных заявок");
            return await _repository.FindPendingBookings();
        }

        /// <summary>
        /// Метод получения конкретной брони по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор брони</param>
        /// <returns>Возвращает экземпляр BookingModel в случае нахождения в противном случае null </returns>
        public async Task<BookingModel?> FindById(Guid id)
        {
            _logger.LogDebug("Попытка найти Booking с ID = {id}", id);

            var findBooking = await _repository.FindById(id);
            if (findBooking == null)
            {
                _logger.LogWarning("[BookingController] Бронь {BookingId} не найдена", id);
                throw new KeyNotFoundException($"Бронь не найдена");
            }
            _logger.LogInformation($"Получилось найти Booking с ID = {id}", id);

            return findBooking;
        }

        /// <summary>
        /// Добавление новой брони
        /// </summary>
        /// <param name="entity">Принимает модель брони</param>
        /// <returns>Возвращает уникальный идентификатор новой брони</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, в случае если передана пустая модель</exception>
        public async Task<Guid> Create(BookingModel entity)
        {
            _logger.LogDebug("Попытка Create Booking. entity = {@entity} ", entity);
            if (entity is null)
            {
                _logger.LogError("Ошибка Booking. попытка передать null сущность");
                throw new ArgumentNullException(nameof(entity));
            }

            await _repository.Create(entity);
            _logger.LogInformation("Создание нового Booking с Id: {Id} ", entity.Id);

            return entity.Id;
        }

        public async Task<BookingModel> CreateBookingAsync(Guid eventId)
        {
            _logger.LogInformation("Попытка создать бронь для события {EventId}", eventId);
            var findEvent = await _eventService.FindById(eventId);
            if (findEvent == null)
            {
                throw new KeyNotFoundException("Событие не найдено");
            }

            var newBooking = new BookingModel { EventId = eventId };

            await _repository.Create(newBooking);
            _logger.LogInformation("Бронь создана: {Id}", newBooking.Id);
            return newBooking;
        }

        /// <summary>
        /// Метод изменения существующей брони
        /// </summary>
        /// <param name="entity">Принимает модель брони</param>
        /// <returns>Возвращает обновлённую модель брони</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="KeyNotFoundException">Выбрасывается в случае, если такой брони по ID не найдено</exception>
        public async Task<BookingModel> Update(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("Ошибка Update. попытка передать null сущность");
                throw new ArgumentNullException(nameof(entity));
            }

            var findBooking = await _repository.FindById(entity.Id);
            if (findBooking is null)
            {
                _logger.LogError("Ошибка Update. Booking с ID: {id}, не найдено!", entity.Id);
                throw new KeyNotFoundException("Event не найден!");
            }

            _logger.LogDebug("Попытка обновления Booking. entity = {@entity} ", findBooking);

            await _repository.Update(entity);

            _logger.LogInformation("Booking обновлён. Новые данные: entity = {@entity} ", entity);

            return entity;
        }

        /// <summary>
        /// Метод удаления брони
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="KeyNotFoundException">Выбрасывается в случае, если тако по ID не найдено</exception>
        public async Task Delete(BookingModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("Ошибка Delete. попытка передать null сущность");
                throw new ArgumentNullException(nameof(entity));
            }

            var findBooking = await _repository.FindById(entity.Id);
            if (findBooking is null)
            {
                _logger.LogWarning("Ошибка Delete. Booking с ID {Id} не найдено", entity.Id);
                throw new KeyNotFoundException("Event не найден!");
            }

            _logger.LogDebug("Попытка Delete Booking. entity = {@entity} ", findBooking);

            await _repository.Delete(findBooking);

            _logger.LogInformation("Booking удалён: id={Id}", findBooking.Id);
        }
    }
}
