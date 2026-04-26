namespace YAP_middle_csharp.Interfaces.IRepositories
{
    /// <summary>
    /// Универсальный интерфейс репозитория для работы с хранилищем данных.
    /// </summary>
    /// <typeparam name="T">Тип хранилища.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Получение всех записей из хранилища
        /// </summary>
        /// <returns>Возвращает все найденные сущности из хранилища</returns>
        Task<IEnumerable<T>> FindAll();

        /// <summary>
        /// Поиск записи по идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <returns>Возвращает найденный тип из хранилища</returns>
        Task<T?> FindById(int id);

        /// <summary>
        /// Сохранение новой сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task Create(T entity);

        /// <summary>
        /// Обновление существующей сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task Update(T entity);

        /// <summary>
        /// Удаление сущности из хранилища
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        Task Delete(T entity);
    }
}
