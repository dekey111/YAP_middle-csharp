using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace YAP_middle_csharp.Interfaces.IRepositories
{
    /// <summary>
    /// Интерфейс для работы с БД событий
    /// </summary>
    public interface IEventRepository : IGenericRepository<EventModel>
    {
    }
}
