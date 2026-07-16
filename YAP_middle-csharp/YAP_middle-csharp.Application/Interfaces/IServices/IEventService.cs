
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Interfaces.IServices
{
    /// <summary>
    /// Интерфейс для работы с бизнес-логикой событий
    /// Объединяет операции чтения и записи для EventModel
    /// </summary>
    public interface IEventService : IQueryService<EventModel>, ICommandService<EventModel>
    {
        Task<EventModel> CreateAsync(EventRequest eventRequest);
        Task<EventModel> UpdateAsync(EventResponse eventResponse);
        Task DeleteAsync(Guid id);
    }
}
