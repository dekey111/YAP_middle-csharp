namespace YAP_middle_csharp.Interfaces.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> FindAll();
        Task<T?> FindById(int id);
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(T entity);

    }
}
