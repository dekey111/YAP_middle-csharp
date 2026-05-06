using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{
    public class BookingRepository : IBooklngRepository
    {
        private readonly List<BookingModel> _bookList = new();

        public Task<IEnumerable<BookingModel>> FindAll()
        {
            return Task.FromResult(_bookList.AsReadOnly() as IEnumerable<BookingModel>);
        }
        public Task<IEnumerable<BookingModel>> FindPendingBookings()
        {
            return Task.FromResult(_bookList.Where(x => x.Status == BookingStatusEnum.Pending));
        }

        public Task<BookingModel?> FindById(Guid id)
        {
            return Task.FromResult(_bookList.FirstOrDefault(x => x.Id == id));
        }

        public Task Create(BookingModel entity)
        {
            entity.Id = Guid.NewGuid();
            _bookList.Add(entity);
            return Task.CompletedTask;
        }

        public Task Update(BookingModel entity)
        {
            var findIndex = _bookList.FindIndex(x => x.Id == entity.Id);
            if (findIndex != -1)
                _bookList[findIndex] = entity;
            return Task.CompletedTask;
        }

        public Task Delete(BookingModel entity)
        {
            _bookList.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
