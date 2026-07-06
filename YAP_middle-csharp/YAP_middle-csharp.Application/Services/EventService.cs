using Microsoft.Extensions.Logging;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Services
{
    /// <summary>
    /// Сервис с бизнес логикой для работы с Event
    /// </summary>
    /// <param name="repository">Принимает контракт от репозитория</param>
    /// <param name="logger">Принимает реализацию логирования</param>
    public class EventService(IEventRepository repository, IValidator<EventModel> validator, ILogger<EventService> logger) : IEventService
    {
        private readonly IEventRepository _repository = repository;
        IValidator<EventModel> _validator = validator;
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
        /// <exception cref="ValidationExceptionApp">Выбрасывается, если параметры пагинации вне допустимого диапазона</exception>
        public async Task<PaginatedResult<EventModel>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null,
            int page = 1, int pageSize = 10)
        {
            _logger.LogDebug("[EventService] [FindAll] Начало выполнения FindAll: title={Title}, from={From}, to={To}, page={Page}, pSize={pSize}",
                title, from, to, page, pageSize);

            if (page < 1)
            {
                _logger.LogWarning("[EventService] [FindAll] Передан некорректный номер страницы: {Page}", page);
                throw new ValidationExceptionApp("Номер страницы должен быть не менее 1");
            }

            if (pageSize < 1 || pageSize > 200)
            {
                _logger.LogWarning("[EventService] [FindAll] Передан некорректный размер страницы: {PageSize}", pageSize);
                throw new ValidationExceptionApp("Размер страницы должен быть от 1 до 200");
            }

            var result = await _repository.GetPagedAsync(title, from, to, page, pageSize);

            _logger.LogInformation("[EventService] [FindAll] Выполнен FindAll. Получено строк: {TotalCount}", result.TotalCount);

            return result;
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор события</param>
        /// <returns>Возвращает экземпляр EventModel в случае нахождения в противном случае null </returns>
        public async Task<EventModel?> FindByIdAsync(Guid id)
        {
            _logger.LogDebug("[EventService] [FindById] Попытка найти Event с ID = {id}", id);
            
            var findEvent = await _repository.FindByIdAsync(id);

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
        public async Task<EventModel> CreateAsync(EventRequest eventRequest)
        {
            _logger.LogDebug("[EventService] [Create] Попытка создания Event");

            if (eventRequest is null)
            {
                throw new ValidationExceptionApp("Запрос на создание события не может быть пустым");
            }

            var eventModel = new EventModel()
            {
                Id = Guid.NewGuid(),
                Title = eventRequest.Title,
                Description = eventRequest.Description,
                TotalSeats = eventRequest.TotalSeats ?? 0,
                AvailableSeats = eventRequest.TotalSeats ?? 0,
                StartAt = eventRequest.StartAt,
                EndAt = eventRequest.EndAt
            };

            var errors = _validator.GetErrors(eventModel).ToList();
            if (errors.Any())
            {
                _logger.LogDebug(string.Join("; ", errors), "[EventService] [Create] Ошибка валидации");
                throw new ValidationExceptionApp(string.Join("; ", errors));
            }

            await _repository.CreateAsync(eventModel);
            _logger.LogInformation("[EventService] [Create] Создано Event ID: {Id}", eventModel.Id);

            return eventModel;
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="entity">Принимает модель события</param>
        /// <returns>Возвращает обновлённую модель</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task<EventModel> UpdateAsync(EventResponse eventResponse)
        {
            if (eventResponse is null)
            {
                throw new ValidationExceptionApp("Данные для обновления не могут быть пустыми");
            }

            var findEvent = await _repository.FindByIdAsync(eventResponse.Id);
            if (findEvent is null)
            {
                _logger.LogError("[EventService] [Update] Event ID: {id} не найдено!", eventResponse.Id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            findEvent.Title = eventResponse.Title;
            findEvent.Description = eventResponse.Description;
            findEvent.TotalSeats = eventResponse.TotalSeats;
            findEvent.StartAt = eventResponse.StartAt;
            findEvent.EndAt = eventResponse.EndAt;

            var errors = _validator.GetErrors(findEvent).ToList();
            if (errors.Any())
            {
                throw new ValidationExceptionApp(string.Join("; ", errors));
            }

            await _repository.UpdateAsync(findEvent);
            _logger.LogInformation("[EventService] [Update] Event ID: {Id}, успешно обновлён", findEvent.Id);

            return findEvent;
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task DeleteAsync(Guid id)
        {
            _logger.LogDebug("[EventService] [Delete] Попытка Delete Event ID = {Id}", id);

            var findEvent = await _repository.FindByIdAsync(id);
            if (findEvent is null)
            {
                _logger.LogInformation("[EventService] [Delete] Event ID: {id} не найден!", id);
                throw new NotFoundExceptionApp($"Event ID: {id} не найден!");
            }

            await _repository.DeleteAsync(findEvent);
            _logger.LogInformation("[EventService] [Delete] Event ID: {Id}, успешно удалён!", id);
        }

        /// <summary>
        /// Реализация контранкта с ICommandService<EventModel>
        /// </summary>
        /// <param name="entity">Принимает сущность EventModel</param>
        /// <returns></returns>
        /// <exception cref="ValidationExceptionApp">Возвращает в случае если переданная сущность is null</exception>
        public async Task<Guid> CreateAsync(EventModel entity)
        {
            if (entity is null) throw new ValidationExceptionApp(nameof(entity));
            await _repository.CreateAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// Реализация контранкта с ICommandService<EventModel>
        /// </summary>
        /// <param name="entity">Принимает сущность EventModel</param>
        /// <returns></returns>
        /// <exception cref="ValidationExceptionApp">Возвращает в случае если переданная сущность is null</exception>
        public async Task<EventModel> UpdateAsync(EventModel entity)
        {
            if (entity is null) throw new ValidationExceptionApp(nameof(entity));
            await _repository.UpdateAsync(entity);
            return entity;
        }

        /// <summary>
        /// Реализация контранкта с ICommandService<EventModel>
        /// </summary>
        /// <param name="entity">Принимает сущность EventModel</param>
        /// <returns></returns>
        /// <exception cref="ValidationExceptionApp">Возвращает в случае если переданная сущность is null</exception>
        public async Task DeleteAsync(EventModel entity)
        {
            if (entity is null) throw new ValidationExceptionApp(nameof(entity));
            await _repository.DeleteAsync(entity);
        }
    }
}
