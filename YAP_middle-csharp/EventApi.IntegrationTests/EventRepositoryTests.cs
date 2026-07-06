using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.DataAccess;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace EventApi.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class EventRepositoryTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private AppDbContext _context = null!;
        private EventRepository _repository = null!;

        public EventRepositoryTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_fixture.Container.GetConnectionString())
                .Options;

            _context = new AppDbContext(options);

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();

            _repository = new EventRepository(_context);
        }
        public async Task DisposeAsync() => await _context.DisposeAsync();


        [Fact]
        public async Task StartQueryToFindAllAsync_FiltersAndPaginationWork()
        {
            var event1 = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Митап C#",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow
            };


            var event2 = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Митап Java",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow
            };

            var event3 = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Конференция C#",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow
            };

            _context.Events.AddRange(event1, event2, event3);
            await _context.SaveChangesAsync();

            var query = await _repository.StartQueryToFindAllAsync();
            var filteredAndPaginated = await query
                .Where(e => e.Title.Contains("C#"))
                .Skip(0) 
                .Take(1) 
                .ToListAsync();

            Assert.Single(filteredAndPaginated);
            Assert.Contains("C#", filteredAndPaginated.First().Title); 
        }

        [Fact]
        public async Task FindByIdAsync_ReturnsCorrectEvent()
        {
            var testEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Поиск по ID",
                TotalSeats = 50,
                AvailableSeats = 50,
                Description = "Описание",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(2)
            };
            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var result = await _repository.FindByIdAsync(testEvent.Id);

            Assert.NotNull(result);
            Assert.Equal(testEvent.Id, result.Id);
        }

        [Fact]
        public async Task CreateAsync_SavesEventToDatabase()
        {
            var newEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Тестовое событие",
                Description = "Описание",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            await _repository.CreateAsync(newEvent);

            var savedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == newEvent.Id);
            Assert.NotNull(savedEvent);
            Assert.Equal("Тестовое событие", savedEvent.Title);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingEvent()
        {
            var testEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Старое название",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1)
            };
            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            testEvent.Title = "Новое название";
            await _repository.UpdateAsync(testEvent);

            var updatedEvent = await _context.Events.FindAsync(testEvent.Id);
            Assert.NotNull(updatedEvent);
            Assert.Equal("Новое название", updatedEvent.Title);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEventFromDatabase()
        {
            var testEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                Title = "Удаляемое событие",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1)
            };
            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(testEvent);

            var deletedEvent = await _context.Events.FindAsync(testEvent.Id);
            Assert.Null(deletedEvent);
        }
    }
}
