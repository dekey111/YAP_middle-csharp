using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly List<BookingModel> _bookList = new();

        //public Task<IEnumerable<BookingModel>> FindAll()
        //{
        //    return Task.FromResult(_bookList.AsReadOnly() as IEnumerable<BookingModel>);
        //}

        /// <summary>
        /// Метод-заготовка для получения данных с фильтрацией на стороне БД
        /// </summary>
        /// <returns></returns>
        public Task<IQueryable<BookingModel>> StartQueryToFindAll()
        {
            return Task.FromResult(_bookList.AsQueryable());
        }

        /// <summary>
        /// Метод для нахождения необработанных броней
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<BookingModel>> FindPendingBookings()
        {
            return Task.FromResult(_bookList.Where(x => x.Status == BookingStatusEnum.Pending));
        }

        /// <summary>
        /// Метод для нахождения конкретной брони
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<BookingModel?> FindById(Guid id)
        {
            return Task.FromResult(_bookList.FirstOrDefault(x => x.Id == id));
        }

        /// <summary>
        /// Метод создания нового бронирования
        /// </summary>
        /// <param name="entity">Сущность бронирования</param>
        /// <returns>Сущность бронирования</returns>
        public Task Create(BookingModel entity)
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
        public Task Update(BookingModel entity)
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
        public Task Delete(BookingModel entity)
        {
            _bookList.Remove(entity);
            return Task.CompletedTask;
        }


    }
}
