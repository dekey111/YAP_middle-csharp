using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.DataAccess;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{

    /// <summary>
    /// Реализация работы с БД событий
    /// </summary>
    public class EventRepository(AppDbContext context) : IEventRepository
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        /// Метод-заготовка для получения данных с фильтрацией на стороне БД
        /// </summary>
        /// <returns></returns>
        public Task<IQueryable<EventModel>> StartQueryToFindAllAsync()
        {
            return Task.FromResult(_context.Events.AsQueryable());
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
