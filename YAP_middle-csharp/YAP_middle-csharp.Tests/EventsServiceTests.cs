using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Application.Validator;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace YAP_middle_csharp.Tests
{
    public class EventsServiceTests
    {
        private readonly IEventService _eventService;
        private readonly ICommandService<EventModel> _commandService;
        private readonly AppDbContext _context;
        private readonly EventValidator _validator;
        private readonly ServiceProvider _serviceProvider;

        public EventsServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddLogging();

            services.AddTransient<IValidator<EventModel>, EventValidator>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();

            _serviceProvider = services.BuildServiceProvider();

            var scope = _serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            _commandService = (ICommandService<EventModel>)_eventService;

            _validator = new EventValidator();
        }

        #region Успешные сценарии:
        [Fact]
        public async Task Create_ReturnNewId()
        {
            var newEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                TotalSeats = 2,
                Title = "Хакатон",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _commandService.CreateAsync(newEvent);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public async Task FindAll_ReturnAllEventWithPagination()
        {
            var allEvents = await _eventService.FindAllAsync();
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
            var id = await _commandService.CreateAsync(newEvent);
            var findEvent = await _eventService.FindByIdAsync(id);

            Assert.NotNull(findEvent);
            Assert.Equal("Рок концерт", findEvent?.Title);
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
            var id = await _commandService.CreateAsync(newEvent);
            _context.ChangeTracker.Clear();

            var eventToUpdate = new EventModel
            {
                Id = id,
                TotalSeats = 2,
                Title = "Курсы C#",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            await _commandService.UpdateAsync(eventToUpdate);

            _context.ChangeTracker.Clear();
            var result = await _eventService.FindByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Курсы C#", result?.Title);
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

            var id = await _commandService.CreateAsync(newEvent);
            newEvent.Id = id;

            _context.ChangeTracker.Clear();
            await _commandService.DeleteAsync(newEvent);

            _context.ChangeTracker.Clear();
            var result = await _eventService.FindByIdAsync(id);
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
            await _commandService.CreateAsync(newEvent);

            var result = await _eventService.FindAllAsync(title: "C#");
            Assert.Equal("Курсы по C#", result.Items.First().Title);
        }

        [Fact]
        public async Task FilterByDates_ReturnsMatchingEvents()
        {
            var today = DateTime.Today;

            var newEvent_BackToFeature = new EventModel
            {
                Id = Guid.NewGuid(),
                TotalSeats = 2,
                Title = "Назад в будущее!",
                StartAt = today.AddDays(-5),
                EndAt = today.AddDays(-4)
            };
            await _commandService.CreateAsync(newEvent_BackToFeature);

            var newEvent_LetGoToPast = new EventModel
            {
                Id = Guid.NewGuid(),
                TotalSeats = 2,
                Title = "Вперед в прошлое!",
                StartAt = today.AddDays(5),
                EndAt = today.AddDays(6)
            };
            await _commandService.CreateAsync(newEvent_LetGoToPast);

            var result = await _eventService.FindAllAsync(from: today);

            Assert.Contains(result.Items, x => x.Title == "Вперед в прошлое!");
        }

        [Fact]
        public async Task Pagination_ReturnCorrectPage()
        {
            for (int i = 1; i <= 15; i++)
                await _commandService.CreateAsync(
                new EventModel
                {
                    Id = Guid.NewGuid(),
                    TotalSeats = 2,
                    Title = $"Я мистер мисикс: {i}, посмотрите на меня!",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow
                });

            var result = await _eventService.FindAllAsync(page: 2, pageSize: 10);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(15, result.TotalCount);
        }

        [Fact]
        public async Task CombinedFilter_ReturnsMatchingEvents()
        {
            var start = new DateTime(2025, 1, 1);
            await _commandService.CreateAsync(new EventModel { Id = Guid.NewGuid(), TotalSeats = 2, Title = "Совместное", StartAt = start, EndAt = start });
            await _commandService.CreateAsync(new EventModel { Id = Guid.NewGuid(), TotalSeats = 2, Title = "Одиночное", StartAt = start, EndAt = start });

            var result = await _eventService.FindAllAsync(title: "Совместное", from: start, to: start);
            Assert.Single(result.Items);
            Assert.Equal("Совместное", result.Items.First().Title);
        }
        #endregion

        #region Неуспешные сценарии
        [Fact]
        public async Task FailedFindById_ShouldReturnNull_ForNonExistentId()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _eventService.FindByIdAsync(nonExistentId);
            Assert.Null(result);
        }

        [Fact]
        public async Task FailedUpdate_ShouldThrowDbUpdateConcurrencyException_ForNonExistentId()
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
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _commandService.UpdateAsync(newEvent));
        }

        [Fact]
        public async Task FailedCreate_WhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ValidationExceptionApp>(() => _commandService.CreateAsync(null!));
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