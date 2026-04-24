using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace YAP_middle_csharp.Services
{
    public class EventService(IRepository<EventResponse> repository, ILogger<EventService> logger) : IEventService
    {
        private readonly IRepository<EventResponse> _repository = repository;
        private readonly ILogger<EventService> _logger = logger;


        public async Task<PaginatedResult<EventResponse>> FindAll(string? title = null, DateTime? from = null, DateTime? to = null,
            int page = 1, int pageSize = 10)
        {
            _logger.LogDebug("Начало выполнения FindAll: title={Title}, from={From}, to={To}, page ={Page}, pSize={pSize}",
                title, from, to, page, pageSize);

            if(page < 1)
            {
                throw new ArgumentException("Номер страницы должен быть не менее 1");
            }

            if (pageSize < 1 || pageSize > 200)
            {
                throw new ArgumentException("Размер страницы должен быть от 1 до 200");
            }

            var findAllEvents = await _repository.FindAll();
            var query = findAllEvents.AsEnumerable();

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
            query = query.OrderByDescending(x => x.EndAt).Skip((page - 1) * pageSize).Take(pageSize);

            var result = new PaginatedResult<EventResponse>
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

        public async Task<EventResponse?> FindById(int id)
        {
            _logger.LogDebug("Попытка найти Event с ID ={i}", id);
            
            var findEvent = await _repository.FindById(id);

            var comment = findEvent is null ? "Не получилось" : "Получилось";
            _logger.LogInformation($"{comment} найти Event с ID = {id}", id);

            return findEvent;
        }

        public async Task<int> Create(EventResponse entity)
        {
            _logger.LogDebug("Попытка создания нового Event. entity = {@entity} ", entity);

            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            
            await _repository.Create(entity);
            _logger.LogInformation("Создание нового Event с Id: {Id} ", entity.Id);

            return entity.Id;
        }

        public async Task<EventResponse> Update(EventResponse entity)
        {

            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var findEvent = await _repository.FindById(entity.Id);
            if (findEvent is null)
                throw new KeyNotFoundException("Event не найден!");

            _logger.LogDebug("Попытка обновления Event. entity = {@entity} ", findEvent);

            await _repository.Update(entity);

            _logger.LogInformation("Event обновлён. Новые данные: entity = {@entity} ", entity);

            return entity;
        }

        public async Task Delete(EventResponse entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var findEvent = await _repository.FindById(entity.Id);
            if (findEvent is null)
                throw new KeyNotFoundException("Event не найден!");

            _logger.LogDebug("Попытка удаления Event. entity = {@entity} ", findEvent);

            await _repository.Delete(findEvent);

            _logger.LogInformation("Event удалён: id={Id}", findEvent.Id);
        }
    }
}
