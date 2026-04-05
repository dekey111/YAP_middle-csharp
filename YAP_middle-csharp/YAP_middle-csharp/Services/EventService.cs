using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Services
{
    public class EventService : IEventService
    {
        private readonly static List<EventResponse> _items = new();
        public Task<IEnumerable<EventResponse>> FindAll()
        {
            return Task.FromResult<IEnumerable<EventResponse>>(_items.ToList());
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
                throw new Exception("Event не найден!");

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
                throw new Exception("Event не найден!");

            _items.Remove(findEvent);
            
            return Task.CompletedTask;
        }
    }
}
