using System.Security.Principal;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces
{
    public interface IQueryService<T> where T : class
    {
        Task<T?> FindById(int id);
        Task<PaginatedResult<T>> FindAll(string? title, DateTime? from, DateTime? to, int page, int pageSize);
    }
}
