using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces.IServices
{
    /// <summary>
    /// Интерфейс для работы с бизнес-логикой событий
    /// Объединяет операции чтения и записи для EventModel
    /// </summary>
    public interface IEventService : IQueryService<EventModel>, ICommandService<EventModel>
    {
        
    }
}
