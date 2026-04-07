using System.Security.Principal;

namespace YAP_middle_csharp.Interfaces
{
    public interface ICommandService<T> where T : class
    {
        Task<int> Create(T entity);
        Task<T>Update(T entity);
        Task Delete(T entity);
    }
}
