using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Models;
using YAP_middle_csharp_Booking.Domain.Models;
using YAP_middle_csharp_Booking.Infrastructure.DataAccess;

namespace YAP_middle_csharp_Booking.Infrastructure.Repository
{
    public class BookingRepository(AppDbContext context) : IBookingRepository
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
        public async Task<PaginatedResult<BookingModel>> GetPagedAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Bookings.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                if (Enum.TryParse<BookingStatusEnum>(title, true, out var statusFilter))
                {
                    query = query.Where(x => x.Status == statusFilter);
                }
            }
            if (from.HasValue && from is not null)
            {
                query = query.Where(x => x.CreatedAt.Date == from.Value.Date);
            }
            if (to.HasValue && to is not null)
            {
                query = query.Where(x => x.ProcessedAt != null && x.ProcessedAt.Value.Date == to.Value.Date);
            }
            var totalCount = await query.CountAsync(cancellationToken);
            var resultQuery = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginatedResult<BookingModel>
            {
                Items = resultQuery,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Метод для нахождения необработанных броней
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BookingModel>> FindPendingBookingsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .Where(x => x.Status == BookingStatusEnum.Pending)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Метод для нахождения конкретной брони
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BookingModel?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Метод создания нового бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns>Сущность бронирования</returns>
        public async Task CreateAsync(BookingModel entity, CancellationToken cancellationToken = default)
        {
            await _context.Bookings.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Метод обновления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        public async Task UpdateAsync(BookingModel entity, CancellationToken cancellationToken = default)
        {
            _context.Bookings.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Метод удаления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        public async Task DeleteAsync(BookingModel entity, CancellationToken cancellationToken = default)
        {
            _context.Bookings.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CheckActiveCountBookingByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.CountAsync(x => x.UserId == userId &&
            (x.Status == BookingStatusEnum.Pending || x.Status == BookingStatusEnum.Confirmed), cancellationToken);
        }
    }

}
