using System.Security.Principal;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Interfaces.IServices
{
    /// <summary>
    /// Интерфейс сервиса для чтения сущностей
    /// </summary>
    /// <typeparam name="T">Тип модели</typeparam>
    public interface IQueryService<T> where T : class
    {
        /// <summary>
        /// Получение сущности по уникальному идентификатору
        /// </summary>
        /// <param name="id">уникальной идентификатор</param>
        /// <returns>Возвращает найденную сущность или null, если ничего не найдено</returns>
        Task<T?> FindById(int id);

        /// <summary>
        /// Получение списка сущностей с поддержкой фильтрации и пагинации
        /// </summary>
        /// <param name="title">Фильтр по наименованию</param>
        /// <param name="from">Начальная дата для фильтрации</param>
        /// <param name="to">Конечная дата для фильтрации</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="pageSize">Количество элементов на странице</param>
        /// <returns>Объект с результатами пагинации</returns>
        Task<PaginatedResult<T>> FindAll(string? title = null,
                                         DateTime? from = null,
                                         DateTime? to = null,
                                         int page = 1,
                                         int pageSize = 10);
    }
}
