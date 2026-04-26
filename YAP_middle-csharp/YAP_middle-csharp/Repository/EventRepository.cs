using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    public class EventRepository : IRepository<EventModel>
    {
        private readonly List<EventModel> _eventList = new();

        public Task<IEnumerable<EventModel>> FindAll()
        {
            return Task.FromResult(_eventList.AsReadOnly() as IEnumerable<EventModel>);
        }

        public Task<EventModel?> FindById(int id)
        {
            return Task.FromResult(_eventList.FirstOrDefault(x => x.Id == id));
        }

        public Task Create(EventModel item)
        {
            item.Id = _eventList.Any() ? _eventList.Max(x => x.Id) + 1 : 1;
            _eventList.Add(item);
            return Task.CompletedTask;
        }

        public Task Update(EventModel item)
        {
            var index = _eventList.FindIndex(x => x.Id == item.Id);
            if (index != -1) 
                _eventList[index] = item;
            return Task.CompletedTask;
        }

        public Task Delete(EventModel item)
        {
            _eventList.Remove(item);
            return Task.CompletedTask;
        }
    }
}
