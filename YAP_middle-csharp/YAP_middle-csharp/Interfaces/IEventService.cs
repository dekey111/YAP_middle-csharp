using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventModel>> GetAll();
        Task<EventModel?> GetById(int id);
        Task<EventModel> Add(EventModel eventModel);
        Task<EventModel> Edit(EventModel eventModel);
        Task Delete(int id);
    }
}
