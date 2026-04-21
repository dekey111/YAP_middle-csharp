using System.Security.Principal;

namespace YAP_middle_csharp.Interfaces
{
    public interface IQueryService<T> where T : class
    {
        Task<T?> FindById(int id);
        Task<IEnumerable<T>> FindAll(string? title, DateTime? from, DateTime? to);
    }
}
