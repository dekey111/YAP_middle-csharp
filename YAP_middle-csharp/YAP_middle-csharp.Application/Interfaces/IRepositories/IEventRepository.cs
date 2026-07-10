using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Интерфейс для работы с БД событий
    /// </summary>
    public interface IEventRepository : IGenericRepository<EventModel>
    {
        Task<PaginatedResult<EventModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize);
    }
}
