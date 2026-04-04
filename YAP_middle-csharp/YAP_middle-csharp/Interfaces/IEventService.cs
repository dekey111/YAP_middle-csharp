using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces
{
    public interface IEventService : IQueryService<EventModel>, ICommandService<EventModel>
    {
    }
}
