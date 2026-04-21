using System.Security.Principal;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces
{
    public interface IQueryService<T> where T : class
    {
        Task<T?> FindById(int id);
        Task<PaginatedResult<T>> FindAll(string? title = null,
                                         DateTime? from = null,
                                         DateTime? to = null,
                                         int page = 1,
                                         int pageSize = 10);
    }
}
