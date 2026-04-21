using System.Net;
using System.Reflection;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Services;

namespace YAP_middle_csharp.Tests
{
    public class EventsServiceTests
    {
        private readonly EventService _eventService;

        public EventsServiceTests()
        {
            _eventService = new EventService();
        }

        #region Успешные сценарии:
        [Fact]
        public async Task Create_ReturnNewId()
        {
            var newEvent = new EventResponse { Title = "Хакатон", StartAt = DateTime.Now, EndAt  = DateTime.Now.AddMonths(1)};
            var id = await _eventService.Create(newEvent);
            Assert.True(id > 0);
        }

        [Fact]
        public async Task FindAll_ReturnAllEventWitchPagination()
        {
            var allEvents = await _eventService.FindAll();
            Assert.True(allEvents.TotalCount > 0);
        }

        [Fact]
        public async Task FindEventById_ReturnExist()
        {
            var newEvent = new EventResponse { Title = "Рок концерт", StartAt = DateTime.Now.AddMonths(2), EndAt = DateTime.Now.AddMonths(3) };
            var id = await _eventService.Create(newEvent);
            var findEvent = await _eventService.FindById(id);

            Assert.NotNull(findEvent);
            Assert.Equal("Рок концерт", findEvent.Title);
        }

        [Fact]
        public async Task UpdateEvent_ReturnUpdateEvent()
        {
            var findEvent = await _eventService.FindById(2);
            findEvent?.Title = "Курсы C#";

            await _eventService.Update(findEvent);
            var result = await _eventService.FindById(findEvent.id);


            Assert.NotNull(result);
            Assert.Equal("Курсы C#", result.Title);
        }

        [Fact]
        public async Task DeleteEvent_ReturnNoContent()
        {
            var newEvent = new EventResponse { Title = "УДАЛИИИТЬ!!!", StartAt = DateTime.Now, EndAt = DateTime.Now };
            var id = await _eventService.Create(newEvent);
            newEvent.id = id;

            await _eventService.Delete(newEvent);
            var result = await _eventService.FindById(id);
            Assert.Null(result);
        }

        [Fact]
        public async Task FilterByTitle_ReturnsMatchingEvents()
        {
            var result = await _eventService.FindAll(title: "c#");
            Assert.Equal("Курсы C#", result.Items.First().Title);
        }

        [Fact]
        public async Task FilterByDates_ReturnsMatchingEvents()
        {
            var today = DateTime.Today;
            await _eventService.Create(new EventResponse { Title = "Назад в будущее!", StartAt = today.AddDays(-5), EndAt = today.AddDays(-4) });
            await _eventService.Create(new EventResponse { Title = "В будущее назад!", StartAt = today.AddDays(5), EndAt = today.AddDays(6) });

            var result = await _eventService.FindAll(from: today);

            Assert.Contains(result.Items, x => x.Title == "В будущее назад!");
        }

        [Fact]
        public async Task Pagination_ReturnCorrectPage()
        {
            for (int i = 1; i <= 15; i++)
                await _eventService.Create(new EventResponse { Title = $"E{i}", StartAt = DateTime.Now, EndAt = DateTime.Now });

            var result = await _eventService.FindAll(page: 2, pageSize: 10);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(15, result.TotalCount);
        }

        [Fact]
        public async Task CombinedFilter_ReturnsMatchingEvents()
        {
            var start = new DateTime(2025, 1, 1);
            await _eventService.Create(new EventResponse { Title = "Совместное", StartAt = start, EndAt = start });
            await _eventService.Create(new EventResponse { Title = "Одиночное", StartAt = start, EndAt = start });

            var result = await _eventService.FindAll(title: "Совместное", from: start, to: start);
            Assert.Single(result.Items);
            Assert.Equal("Совместное", result.Items.First().Title);
        }
        #endregion

        #region Неуспешные сценарии
        [Fact]
        public async Task FailedFindById_ShouldReturnNull_ForNonExistentId()
        {
            var result = await _eventService.FindById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task FailedUpdate_ShouldThrowKeyNotFound_ForNonExistentId()
        {
            var ev = new EventResponse { id = 999, Title = "меня не существует", StartAt = DateTime.Now, EndAt = DateTime.Now};
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _eventService.Update(ev));
        }

        [Fact]
        public async Task FailedCreate_WhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _eventService.Create(null!));
        }

        [Fact]
        public async Task FailedUpdate_WhenDatesInvalid()
        {
            var id = await _eventService.Create(new EventResponse { Title = "Валид", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1) });
            var invalid = new EventResponse { id = id, Title = "Инвалид", StartAt = DateTime.Now, EndAt = default };
        }
        #endregion

    }
}
