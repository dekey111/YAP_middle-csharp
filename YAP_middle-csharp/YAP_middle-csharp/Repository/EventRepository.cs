using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Repository
{

    /// <summary>
    /// Реализация работы с БД событий
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly List<EventModel> _eventList = new();

        //public Task<IEnumerable<EventModel>> FindAll()
        //{
        //    return Task.FromResult(_eventList.AsReadOnly() as IEnumerable<EventModel>);
        //}


        /// <summary>
        /// Метод-заготовка для получения данных с фильтрацией на стороне БД
        /// </summary>
        /// <returns></returns>
        public Task<IQueryable<EventModel>> StartQueryToFindAllAsync()
        {
            return Task.FromResult(_eventList.AsQueryable());
        }

        /// <summary>
        /// Метод для нахождения конкретного события
        /// </summary>
        /// <param name="id">УИ события</param>
        /// <returns>Сущность события</returns>
        public Task<EventModel?> FindByIdAsync(Guid id)
        {
            return Task.FromResult(_eventList.FirstOrDefault(x => x.Id == id));
        }

        /// <summary>
        /// Создание нового события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns>Сущность события</returns>
        public Task CreateAsync(EventModel item)
        {
            item.Id = Guid.NewGuid();
            _eventList.Add(item);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновление события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns></returns>
        public Task UpdateAsync(EventModel item)
        {
            var index = _eventList.FindIndex(x => x.Id == item.Id);
            if (index != -1) 
                _eventList[index] = item;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Удаление события
        /// </summary>
        /// <param name="item">Сущность события</param>
        /// <returns></returns>
        public Task DeleteAsync(EventModel item)
        {
            _eventList.Remove(item);
            return Task.CompletedTask;
        }
    }
}
