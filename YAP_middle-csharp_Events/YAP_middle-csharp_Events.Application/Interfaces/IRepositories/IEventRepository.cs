using YAP_middle_csharp_Events.Application.Models;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Interfaces.IRepositories
{
    public interface IEventRepository
    {
        /// <summary>
        /// Получение записей с пагинацией страниц
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращает отформатированный список</returns>
        Task<PaginatedResult<EventModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default);


        /// <summary>
        /// Получение 10 самых популярных событий
        /// </summary>
        /// <param name="cancellationToken">токен отмены</param>
        /// <returns></returns>
        Task<IReadOnlyList<EventModel>> FindTop10EventsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Поиск записи по идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <returns>Возвращает найденный тип из хранилища</returns>
        Task<EventModel?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранение новой сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task CreateAsync(EventModel entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновление существующей сущности в хранилище
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>Ничего не возвращает</returns>
        Task UpdateAsync(EventModel entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаление сущности из хранилища
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        /// <returns>Ничего не возвращает</returns>
        Task DeleteAsync(EventModel entity, CancellationToken cancellationToken = default);
    }
}
