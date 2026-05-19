namespace YAP_middle_csharp.Interfaces.IRepositories
{
    /// <summary>
    /// Универсальный интерфейс репозитория для работы с хранилищем данных.
    /// </summary>
    /// <typeparam name="T">Тип хранилища.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Подготовленный запрос для получения сущности 
        /// </summary>
        /// <remarks>
        /// Метод, который возвращает голый запрос для обработки на стороне БД, с возможностью подкрутить необходимые фильтрации
        /// </remarks>
        /// <returns>Возвращает подготовленный шаблон получения данных</returns>
        Task<IQueryable<T>> StartQueryToFindAll();

        /// <summary>
        /// Поиск записи по идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <returns>Возвращает найденный тип из хранилища</returns>
        Task<T?> FindById(Guid id);

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
