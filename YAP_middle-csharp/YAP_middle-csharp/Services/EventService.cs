using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Services
{
    public class EventService : IEventService
    {
        private List<EventModel> _eventModels = new();
        public async Task<IEnumerable<EventModel>> GetAll()
        {
            return _eventModels.ToList();
        }
        public async Task<EventModel?> GetById(int id)
        {
            return _eventModels.Find(x => x.id == id);
        }

        public async Task<EventModel> Add(EventModel eventModel)
        {
            _eventModels.Add(eventModel);
            return eventModel;
        }

        public async Task<EventModel> Edit(EventModel eventModel)
        {
            var findEvent = await GetById(eventModel.id);
            if (findEvent == null)
                throw new Exception("Event не найден!");

            findEvent.Title = eventModel.Title;
            findEvent.Description = eventModel.Description;
            findEvent.StartAt = eventModel.StartAt;
            findEvent.EndAt = eventModel.EndAt;

            return findEvent;
        }

        public async Task Delete(int id)
        {
            var findEvent = await GetById(id);
            if (findEvent == null)
                throw new Exception("Event не найден!");

            _eventModels.Remove(findEvent);
        }
    }
}
