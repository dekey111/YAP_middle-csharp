using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.DataAccess;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    /// <summary>
    /// Реализация работы с БД бронирований
    /// </summary>
    public class BookingRepository(AppDbContext context) : IBookingRepository
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        /// Метод-заготовка для получения данных с фильтрацией на стороне БД
        /// </summary>
        /// <returns></returns>
        public Task<IQueryable<BookingModel>> StartQueryToFindAllAsync()
        {
            return Task.FromResult(_context.Bookings.AsQueryable());
        }

        /// <summary>
        /// Метод для нахождения необработанных броней
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBookingsAsync()
        {
            return await _context.Bookings
                .Where(x => x.Status == BookingStatusEnum.Pending)
                .ToListAsync();
        }

        /// <summary>
        /// Метод для нахождения конкретной брони
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BookingModel?> FindByIdAsync(Guid id)
        {
            return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Метод создания нового бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns>Сущность бронирования</returns>
        public async Task CreateAsync(BookingModel entity)
        {
            _context.Bookings.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Метод обновления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        public async Task UpdateAsync(BookingModel entity)
        {
            _context.Bookings.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Метод удаления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        public async Task DeleteAsync(BookingModel entity)
        {
            _context.Bookings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
