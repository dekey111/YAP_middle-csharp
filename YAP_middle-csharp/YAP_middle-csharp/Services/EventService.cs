using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Services
{
    public class EventService : IEventService
    {
        private readonly static List<EventResponse> _items = new();
        public Task<PaginatedResult<EventResponse>> FindAll(string? title, DateTime? from, DateTime? to,
            int page, int pageSize)
        {
            var query = _items.AsEnumerable();

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

            return Task.FromResult(result);
        }

        public Task<EventResponse?> FindById(int id)
        {
            var item = _items.FirstOrDefault(x => x.id == id);
            return Task.FromResult(item);
        }

        public Task<int> Create(EventResponse entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.id = _items.Any() ? _items.Max(x => x.id) + 1 : 1;
            _items.Add(entity);
            
            return Task.FromResult(entity.id);
        }

        public Task<EventResponse> Update(EventResponse entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var findEvent = _items.FirstOrDefault(x => x.id == entity.id);
            if (findEvent == null)
                throw new KeyNotFoundException("Event не найден!");

            findEvent.Title = entity.Title;
            findEvent.Description = entity.Description;
            findEvent.StartAt = entity.StartAt;
            findEvent.EndAt = entity.EndAt;

            return Task.FromResult(findEvent);
        }

        public Task Delete(EventResponse entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var findEvent = _items.FirstOrDefault(x => x.id == entity.id);
            if (findEvent == null)
                throw new KeyNotFoundException("Event не найден!");

            _items.Remove(findEvent);
            
            return Task.CompletedTask;
        }
    }
}
