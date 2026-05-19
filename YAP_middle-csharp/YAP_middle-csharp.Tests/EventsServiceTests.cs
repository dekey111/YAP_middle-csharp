using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Reflection;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;
using YAP_middle_csharp.Services;
using YAP_middle_csharp.Validator;

namespace YAP_middle_csharp.Tests
{
    public class EventsServiceTests
    {
        private readonly EventService _eventService;
        private readonly EventValidator _validator;

        public EventsServiceTests()
        {
            var repository = new EventRepository();
            var logger = new NullLogger<EventService>();

            _eventService = new EventService(repository, logger);
            _validator = new EventValidator();
        }

        #region Успешные сценарии:
        [Fact]
        public async Task Create_ReturnNewId()
        {
            var newEvent = new EventModel 
            {
                TotalSeats = 2,
                Title = "Хакатон", 
                StartAt = DateTime.UtcNow, 
                EndAt = DateTime.UtcNow.AddMonths(1) 
            };
            var id = await _eventService.Create(newEvent);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public async Task FindAll_ReturnAllEventWithPagination()
        {
            var allEvents = await _eventService.FindAll();
            Assert.True(allEvents.TotalCount == 0);
        }

        [Fact]
        public async Task FindEventById_ReturnExist()
        {
            var newEvent = new EventModel 
            {
                TotalSeats = 2,
                Title = "Рок концерт", 
                StartAt = DateTime.UtcNow.AddMonths(2),
                EndAt = DateTime.UtcNow.AddMonths(3) 
            };
            var id = await _eventService.Create(newEvent);
            var findEvent = await _eventService.FindById(id);

            Assert.NotNull(findEvent);
            Assert.Equal("Рок концерт", findEvent.Title);
        }

        [Fact]
        public async Task UpdateEvent_ReturnUpdateEvent()
        {
            var newEvent = new EventModel
            {
                Title = "Курсы не по C#",
                TotalSeats = 2,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };
            var id = await _eventService.Create(newEvent);

            var eventToUpdate = new EventModel
            {
                Id = id,
                TotalSeats = 2,
                Title = "Курсы C#",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            await _eventService.Update(eventToUpdate);
            var result = await _eventService.FindById(id);

            Assert.NotNull(result);
            Assert.Equal("Курсы C#", result.Title);
        }

        [Fact]
        public async Task DeleteEvent_ReturnNoContent()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                Title = "УДАЛИИИТЬ!!!",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow
            };

            var id = await _eventService.Create(newEvent);
            newEvent.Id = id;

            await _eventService.Delete(newEvent);
            var result = await _eventService.FindById(id);
            Assert.Null(result);
        }

        [Fact]
        public async Task FilterByTitle_ReturnsMatchingEvents()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                Title = "Курсы по C#",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };
            await _eventService.Create(newEvent);

            var result = await _eventService.FindAll(title: "c#");
            Assert.Equal("Курсы по C#", result.Items.First().Title);
        }

        [Fact]
        public async Task FilterByDates_ReturnsMatchingEvents()
        {
            var today = DateTime.Today;

            var newEvent_BackToFeature = new EventModel
            {
                TotalSeats = 2,
                Title = "Назад в будущее!",
                StartAt = today.AddDays(-5),
                EndAt = today.AddDays(-4)
            };
            await _eventService.Create(newEvent_BackToFeature);

            var newEvent_LetGoToPast = new EventModel
            {
                TotalSeats = 2,
                Title = "Вперед в прошлое!",
                StartAt = today.AddDays(5),
                EndAt = today.AddDays(6)
            };
            await _eventService.Create(newEvent_LetGoToPast);

            var result = await _eventService.FindAll(from: today);

            Assert.Contains(result.Items, x => x.Title == "Вперед в прошлое!");
        }

        [Fact]
        public async Task Pagination_ReturnCorrectPage()
        {
            for (int i = 1; i <= 15; i++)
                await _eventService.Create(
                    new EventModel 
                    {
                        TotalSeats = 2,
                        Title = $"Я мистер мисикс: {i}, посмотрите на меня!",
                        StartAt = DateTime.UtcNow, 
                        EndAt = DateTime.UtcNow 
                    });

            var result = await _eventService.FindAll(page: 2, pageSize: 10);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(15, result.TotalCount);
        }

        [Fact]
        public async Task CombinedFilter_ReturnsMatchingEvents()
        {
            var start = new DateTime(2025, 1, 1);
            await _eventService.Create(new EventModel { TotalSeats = 2, Title = "Совместное", StartAt = start, EndAt = start });
            await _eventService.Create(new EventModel { TotalSeats = 2, Title = "Одиночное", StartAt = start, EndAt = start });

            var result = await _eventService.FindAll(title: "Совместное", from: start, to: start);
            Assert.Single(result.Items);
            Assert.Equal("Совместное", result.Items.First().Title);
        }
        #endregion

        #region Неуспешные сценарии
        [Fact]
        public async Task FailedFindById_ShouldReturnNull_ForNonExistentId()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _eventService.FindById(nonExistentId);
            Assert.Null(result);
        }

        [Fact]
        public async Task FailedUpdate_ShouldThrowKeyNotFound_ForNonExistentId()
        {
            var newEvent = new EventModel 
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Id = Guid.NewGuid(),
                Title = "меня не существует",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow 
            };
            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _eventService.Update(newEvent));
        }

        [Fact]
        public async Task FailedCreate_WhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ValidationExceptionApp>(() => _eventService.Create(null!));
        }

        [Fact]
        public async Task FailedUpdate_WhenDatesInvalid()
        {
            var invalid = new EventModel()
            {
                TotalSeats = 2,
                Title = "Инвалид",
                StartAt = DateTime.UtcNow.AddDays(10),
                EndAt = DateTime.UtcNow
            };

            var errors = _validator.GetErrors(invalid).ToList();

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Дата окончания не может быть раньше даты начала"));
        }
        #endregion
    }
}
