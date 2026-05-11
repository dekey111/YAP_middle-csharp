using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace YAP_middle_csharp.Services
{
    /// <summary>
    /// Сервис с бизнес логикой для работы с Event
    /// </summary>
    /// <param name="repository">Принимает контракт от репозитория</param>
    /// <param name="logger">Принимает реализацию логирования</param>
    public class EventService(IEventRepository repository, ILogger<EventService> logger) : IEventService
    {
        private readonly IEventRepository _repository = repository;
        private readonly ILogger<EventService> _logger = logger;


        /// <summary>
        /// Метод для поиска всех Событий с опциональными фильтрами 
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращается EventModel </returns>
        /// <exception cref="ValidationException">Выбрасывается, если параметры пагинации вне допустимого диапазона</exception>
        public async Task<PaginatedResult<EventModel>> FindAll(string? title = null, DateTime? from = null, DateTime? to = null,
            int page = 1, int pageSize = 10)
        {
            _logger.LogDebug("[EventService] [FindAll] Начало выполнения FindAll: title={Title}, from={From}, to={To}, page ={Page}, pSize={pSize}",
                title, from, to, page, pageSize);

            if(page < 1)
            {
                _logger.LogWarning("[EventService] [FindAll] Передан некорректный номер страницы: {Page}", page);
                throw new ValidationException("Номер страницы должен быть не менее 1");
            }

            if (pageSize < 1 || pageSize > 200)
            {
                _logger.LogWarning("[EventService] [FindAll] Передан некорректный размер страницы: {PageSize}", pageSize);
                throw new ValidationException("Размер страницы должен быть от 1 до 200");
            }

            var query = await _repository.StartQueryToFindAll();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.Trim().Contains(title, StringComparison.OrdinalIgnoreCase));
            }
            if(from.HasValue && from is not null)
            {
                query = query.Where(x => x.StartAt.Date >= from.Value.Date);
            }
            if(to.HasValue && to is not null)
            {
                query = query.Where(x => x.EndAt.Date <= to.Value.Date);
            }
            var totalCount = query.Count();
            var resultQuery = query.OrderByDescending(x => x.EndAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new PaginatedResult<EventModel>
            {
                Items = resultQuery,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            _logger.LogInformation("[EventService] [FindAll] Выполнен FindAll: title={Title}, from={From}, to={To}, page ={Page}, pSize={pSize}. Получилось: {TotalCount}",
                title, from, to, page, pageSize, result.Items.Count());

            return result;
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор события</param>
        /// <returns>Возвращает экземпляр EventModel в случае нахождения в противном случае null </returns>
        public async Task<EventModel?> FindById(Guid id)
        {
            _logger.LogDebug("[EventService] [FindById] Попытка найти Event с ID = {id}", id);
            
            var findEvent = await _repository.FindById(id);

            var comment = findEvent is null ? "Не получилось" : "Получилось";
            _logger.LogInformation($"[EventService] [FindById] {comment} найти Event с ID = {id}", id);

            return findEvent;
        }

        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="entity">Принимает модель события</param>
        /// <returns>Возвращает уникальный идентификатор нового события</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если передана пустая модель</exception>
        public async Task<Guid> Create(EventModel entity)
        {
            _logger.LogDebug("[EventService] [Create] Попытка Create Event. entity = {@entity} ", entity);

            if (entity is null)
            {
                _logger.LogError("[EventService] [Create] Ошибка Create. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            await _repository.Create(entity);
            _logger.LogInformation("[EventService] [Create] Создание нового Event с Id: {Id} ", entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="entity">Принимает модель события</param>
        /// <returns>Возвращает обновлённую модель</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="KeyNotFoundException">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task<EventModel> Update(EventModel entity)
        {

            if (entity is null)
            {
                _logger.LogError("[EventService] [Update] Ошибка Update. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findEvent = await _repository.FindById(entity.Id);
            if (findEvent is null)
            {
                _logger.LogError("[EventService] [Update] Ошибка Update. Событие с ID: {id}, не найдено!", entity.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            _logger.LogDebug("[EventService] [Update] Попытка обновления Event. entity = {@entity} ", findEvent);

            await _repository.Update(entity);

            _logger.LogInformation("[EventService] [Update] Event обновлён. Новые данные: entity = {@entity} ", entity);

            return entity;
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="KeyNotFoundException">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task Delete(EventModel entity)
        {
            if (entity is null)
            {
                _logger.LogError("[EventService] [Delete]  Ошибка Delete. попытка передать null сущность");
                throw new ValidationExceptionApp(nameof(entity));
            }

            var findEvent = await _repository.FindById(entity.Id);
            if (findEvent is null)
            {
                _logger.LogWarning("[EventService] [Delete] Ошибка Delete. Событие с ID {Id} не найдено", entity.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            _logger.LogDebug("[EventService] [Delete] Попытка Delete Event. entity = {@entity} ", findEvent);

            await _repository.Delete(findEvent);

            _logger.LogInformation("[EventService] [Delete] Event удалён: id={Id}", findEvent.Id);
        }
    }
}
