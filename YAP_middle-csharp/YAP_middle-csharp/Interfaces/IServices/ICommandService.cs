using System.Security.Principal;

namespace YAP_middle_csharp.Interfaces.IServices
{
    /// <summary>
    /// Интерфейс сервиса для изменения данных 
    /// </summary>
    /// <typeparam name="T">Тип модели</typeparam>
    public interface ICommandService<T> where T : class
    {
        /// <summary>
        /// Создание новой записи в системе
        /// </summary>
        /// <param name="entity">Объект сущности для сохранения</param>
        /// <returns>Возвращает идентификатор созданной записи</returns>
        Task<int> Create(T entity);

        /// <summary>
        /// Обновление данных существующей записи
        /// </summary>
        /// <param name="entity">Объект сущности с обновленными данными</param>
        /// <returns>Возвращает обновленный объект сущности</returns>
        Task<T>Update(T entity);

        /// <summary>
        /// Удаление записи из системы
        /// </summary>
        /// <param name="entity">Объект сущности для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        Task Delete(T entity);
    }
}
