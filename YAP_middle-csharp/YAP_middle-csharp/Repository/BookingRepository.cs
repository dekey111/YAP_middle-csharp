using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    /// <summary>
    /// Реализация работы с БД бронирований
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly List<BookingModel> _bookList = new();

        /// <summary>
        /// Метод-заготовка для получения данных с фильтрацией на стороне БД
        /// </summary>
        /// <returns></returns>
        public Task<IQueryable<BookingModel>> StartQueryToFindAllAsync()
        {
            return Task.FromResult(_bookList.AsQueryable());
        }

        /// <summary>
        /// Метод для нахождения необработанных броней
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<BookingModel>> FindPendingBookingsAsync()
        {
            return Task.FromResult(_bookList.Where(x => x.Status == BookingStatusEnum.Pending));
        }

        /// <summary>
        /// Метод для нахождения конкретной брони
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<BookingModel?> FindByIdAsync(Guid id)
        {
            return Task.FromResult(_bookList.FirstOrDefault(x => x.Id == id));
        }

        /// <summary>
        /// Метод создания нового бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns>Сущность бронирования</returns>
        public Task CreateAsync(BookingModel entity)
        {
            entity.Id = Guid.NewGuid();
            _bookList.Add(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод обновления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns>Сущность бронирования</returns>
        public Task UpdateAsync(BookingModel entity)
        {
            var findIndex = _bookList.FindIndex(x => x.Id == entity.Id);
            if (findIndex != -1)
                _bookList[findIndex] = entity;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод удаления бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns></returns>
        public Task DeleteAsync(BookingModel entity)
        {
            _bookList.Remove(entity);
            return Task.CompletedTask;
        }


    }
}
