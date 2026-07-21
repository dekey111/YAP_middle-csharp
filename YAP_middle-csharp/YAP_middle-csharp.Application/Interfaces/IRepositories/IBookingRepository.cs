using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;
    
namespace YAP_middle_csharp.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Интерфейс для работы с БД бронированием
    /// </summary>
    public interface IBookingRepository : IGenericRepository<BookingModel>
    {
        /// <summary>
        /// Метод для нахождения необработанных броней
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BookingModel>> FindPendingBookingsAsync();

        /// <summary>
        /// Получение записей с пагинацией страниц
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращает отформатированный список</returns>
        Task<PaginatedResult<BookingModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize);

        /// <summary>
        /// Поиск количества активных броней у пользователя
        /// </summary>
        /// <param name="userId">Принимает УИ пользователя</param>
        /// <returns>Возвращает количетсво активных бронирований у пользователя</returns>
        Task<int> CheckActiveCountBookingByUserId(Guid userId);
    }
}
