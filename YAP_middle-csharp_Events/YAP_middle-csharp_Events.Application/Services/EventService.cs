using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Events.Application.Helper;
using YAP_middle_csharp_Events.Application.Interfaces;
using YAP_middle_csharp_Events.Application.Interfaces.ICache;
using YAP_middle_csharp_Events.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Events.Application.Interfaces.IServices;
using YAP_middle_csharp_Events.Application.Models;
using YAP_middle_csharp_Events.Domain.Exceptions;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Services
{
    public class EventService(IEventRepository repository, IValidator<EventModel> validator, ILogger<EventService> logger, ICacheService cacheService) : IEventService
    {
        private readonly IEventRepository _repository = repository;
        private readonly IValidator<EventModel> _validator = validator;
        private readonly ILogger<EventService> _logger = logger;
        private readonly ICacheService _cacheService = cacheService;

        private static readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Метод для поиска всех Событий с опциональными фильтрами 
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращается EventResponse </returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, если параметры пагинации вне допустимого диапазона</exception>
        public async Task<PaginatedResult<EventContract>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null,
            int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
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

            var result = await _repository.GetPagedAsync(title, from, to, page, pageSize, cancellationToken);

            _logger.LogInformation("[EventService] [FindAll] Выполнен FindAll. Получено строк: {TotalCount}", result.TotalCount);

            var itemsResponse = result.Items.Select(x => x.MapToContract()).ToList();
            return new PaginatedResult<EventContract>
            {
                Items = itemsResponse,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Уникальный идентификатор события</param>
        /// <returns>Возвращает экземпляр EventModel</returns>
        /// <exception cref="NotFoundExceptionApp">В случае если событие не найдено</exception>
        public async Task<EventContract> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"event:{id}";

            var cachedEvent = await _cacheService.GetAsync<EventContract>(cacheKey, cancellationToken);
            if (cachedEvent is not null)
            {
                _logger.LogDebug("[EventService] [FindByIdAsync] Найдено событие в кеше: {Id}", id);
                return cachedEvent;
            }

            var findEvent = await _repository.FindByIdAsync(id, cancellationToken);
            if (findEvent is null)
            {
                _logger.LogInformation("[EventService] [FindByIdAsync] Event ID: {Id} не найден!", id);
                throw new NotFoundExceptionApp($"Event ID: {id} не найден!");
            }

            var responseDto = findEvent.MapToContract();
            await _cacheService.SetAsync(cacheKey, responseDto, _defaultExpiry, cancellationToken);
            _logger.LogDebug("[EventService] [FindByIdAsync] Нашли данные в БД, записали в Кеш и вернули пользователю");
            return responseDto;
        }


        /// <summary>
        /// Получение 10 самых популярных события
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<EventContract>> FindTop10EventsAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "events:top10";

            var findCache = await _cacheService.GetAsync<List<EventContract>>(cacheKey, cancellationToken);
            if (findCache is not null)
            {
                _logger.LogDebug("[EventService] [FindTop10EventsAsync] нашли данные в кеше");
                return findCache;
            }

            var findTop10Db = await _repository.FindTop10EventsAsync(cancellationToken);
            var resultDtos = findTop10Db.Select(x => x.MapToContract()).ToList();
            await _cacheService.SetAsync(cacheKey, resultDtos, _defaultExpiry, cancellationToken);
            _logger.LogDebug("[EventService] [FindTop10EventsAsync] Нашли данные в БД, записали в Кеш и вернули пользователю");

            return resultDtos;
        }

        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="entity">Принимает модель события</param>
        /// <returns>Возвращает уникальный идентификатор нового события</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если передана пустая модель</exception>
        public async Task<Guid> CreateAsync(EventRequest eventRequest, CancellationToken cancellationToken = default)
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

            await _repository.CreateAsync(eventModel, cancellationToken);
            _logger.LogInformation("[EventService] [Create] Создано Event ID: {Id}", eventModel.Id);

            return eventModel.Id;
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="entity">Принимает модель события</param>
        /// <returns>Возвращает обновлённую модель</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task<EventUpdateRequest> UpdateAsync(Guid id, EventUpdateRequest eventUpdateRequest, CancellationToken cancellationToken = default)
        {
            if (eventUpdateRequest is null)
            {
                throw new ValidationExceptionApp("Данные для обновления не могут быть пустыми");
            }

            var findEvent = await _repository.FindByIdAsync(id);
            if (findEvent is null)
            {
                _logger.LogError("[EventService] [Update] Event ID: {id} не найдено!", id);
                throw new NotFoundExceptionApp("Event не найден!");
            }

            findEvent.Title = eventUpdateRequest.Title;
            findEvent.Description = eventUpdateRequest.Description;
            findEvent.TotalSeats = eventUpdateRequest.TotalSeats;
            findEvent.StartAt = eventUpdateRequest.StartAt;
            findEvent.EndAt = eventUpdateRequest.EndAt;

            var errors = _validator.GetErrors(findEvent).ToList();
            if (errors.Any())
            {
                throw new ValidationExceptionApp(string.Join("; ", errors));
            }

            await _repository.UpdateAsync(findEvent, cancellationToken);
            await _cacheService.RemoveAsync($"event:{id}", cancellationToken);
            _logger.LogInformation("[EventService] [Update] Event ID: {Id}, успешно обновлён", findEvent.Id);

            return new EventUpdateRequest(findEvent);
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="entity">Принимает модель для удаления</param>
        /// <returns>Ничего не возвращается</returns>
        /// <exception cref="ValidationExceptionApp">Выбрасывается, в случае если модель пустая</exception>
        /// <exception cref="NotFoundExceptionApp">Выбрасывается в случае, если такого события по ID не найдено</exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("[EventService] [Delete] Попытка Delete Event ID = {Id}", id);

            var findEvent = await _repository.FindByIdAsync(id, cancellationToken);
            if (findEvent is null)
            {
                _logger.LogInformation("[EventService] [Delete] Event ID: {id} не найден!", id);
                throw new NotFoundExceptionApp($"Event ID: {id} не найден!");
            }

            await _repository.DeleteAsync(findEvent);
            await _cacheService.RemoveAsync($"event:{id}", cancellationToken);
            _logger.LogInformation("[EventService] [Delete] Event ID: {Id}, успешно удалён!", id);
        }
    }
}
