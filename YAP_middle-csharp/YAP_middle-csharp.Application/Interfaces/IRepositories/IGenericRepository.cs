namespace YAP_middle_csharp.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Универсальный интерфейс репозитория для работы с хранилищем данных.
    /// </summary>
    /// <typeparam name="T">Тип хранилища.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Поиск записи по идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <returns>Возвращает найденный тип из хранилища</returns>
        Task<T?> FindByIdAsync(Guid id);

        /// <summary>
        /// Сохранение новой сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task CreateAsync(T entity);

        /// <summary>
        /// Обновление существующей сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Удаление сущности из хранилища
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        Task DeleteAsync(T entity);
    }
}
