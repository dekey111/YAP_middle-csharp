using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;


namespace YAP_middle_csharp.Infrastructure.Repository
{

    /// <summary>
    /// Реализация работы с БД событий
    /// </summary>
    public class EventRepository(AppDbContext context) : IEventRepository
    {
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Получение записей с пагинацией страниц
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращает отформатированный список</returns>
        public async Task<PaginatedResult<EventModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.Contains(title));
            }
            if (from.HasValue)
            {
                query = query.Where(x => x.StartAt.Date >= from.Value.Date);
            }
            if (to.HasValue)
            {
                query = query.Where(x => x.EndAt.Date <= to.Value.Date);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.EndAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<EventModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        /// <summary>
        /// Метод для нахождения конкретного события
        /// </summary>
        /// <param name="id">УИ события</param>
        /// <returns>Сущность события</returns>
        public async Task<EventModel?> FindByIdAsync(Guid id)
        {
            return await _context.Events.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Создание нового события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns>Сущность события</returns>
        public async Task CreateAsync(EventModel item)
        {
            _context.Events.Add(item);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновление события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns></returns>
        public async Task UpdateAsync(EventModel item)
        {
            _context.Events.Update(item);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Удаление события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns></returns>
        public async Task DeleteAsync(EventModel item)
        {
            _context.Events.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
