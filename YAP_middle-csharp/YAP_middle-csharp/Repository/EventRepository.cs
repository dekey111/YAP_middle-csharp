using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    public class EventRepository : IRepository<EventResponse>
    {
        private readonly List<EventResponse> _eventList = new();

        public Task<IEnumerable<EventResponse>> FindAll()
        {
            return Task.FromResult(_eventList.AsReadOnly() as IEnumerable<EventResponse>);
        }

        public Task<EventResponse?> FindById(int id)
        {
            return Task.FromResult(_eventList.FirstOrDefault(x => x.Id == id));
        }

        public Task Create(EventResponse item)
        {
            item.Id = _eventList.Any() ? _eventList.Max(x => x.Id) + 1 : 1;
            _eventList.Add(item);
            return Task.CompletedTask;
        }

        public Task Update(EventResponse item)
        {
            var index = _eventList.FindIndex(x => x.Id == item.Id);
            if (index != -1) 
                _eventList[index] = item;
            return Task.CompletedTask;
        }

        public Task Delete(EventResponse item)
        {
            _eventList.Remove(item);
            return Task.CompletedTask;
        }
    }
}
