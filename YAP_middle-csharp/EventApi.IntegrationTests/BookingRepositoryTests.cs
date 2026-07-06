using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.DataAccess;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;

namespace EventApi.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private AppDbContext _context = null!;
        private BookingRepository _repository = null!;

        public BookingRepositoryTests(PostgresFixture fixture)
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
            _repository = new BookingRepository(_context);
        }

        public async Task DisposeAsync() => await _context.DisposeAsync();

        private async Task<Guid> CreateTestEventAsync()
        {
            var eventId = Guid.NewGuid();
            _context.Events.Add(new EventModel
            {
                Id = eventId,
                Title = "Базовое событие",
                Description = "-",
                TotalSeats = 50,
                AvailableSeats = 50,
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return eventId;
        }


        [Fact]
        public async Task StartQueryToFindAllAsync_ReturnsQueryable()
        {
            var eventId = await CreateTestEventAsync();
            var booking1 = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var booking2 = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Rejected,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.AddRange(booking1, booking2);
            await _context.SaveChangesAsync();

            var query = await _repository.StartQueryToFindAllAsync();
            var result = await query.ToListAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task FindPendingBookingsAsync_ReturnsOnlyPending()
        {
            var eventId = await CreateTestEventAsync();
            var pendingBooking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var confirmedBooking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.AddRange(pendingBooking, confirmedBooking);
            await _context.SaveChangesAsync();

            var result = await _repository.FindPendingBookingsAsync();
            var resultList = result.ToList();

            Assert.Single(resultList);
            Assert.Equal(BookingStatusEnum.Pending, resultList.First().Status);
        }

        [Fact]
        public async Task FindByIdAsync_ReturnsCorrectBooking()
        {
            var eventId = await CreateTestEventAsync();
            var booking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _repository.FindByIdAsync(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(booking.Id, result.Id);
        }

        [Fact]
        public async Task CreateAsync_SavesBookingToDatabase()
        {
            var eventId = await CreateTestEventAsync();
            var booking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(booking);

            var savedBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
            Assert.NotNull(savedBooking);
            Assert.Equal(eventId, savedBooking.EventId);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingBooking()
        {
            var eventId = await CreateTestEventAsync();
            var booking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            booking.Status = BookingStatusEnum.Confirmed;
            await _repository.UpdateAsync(booking);

            var updatedBooking = await _context.Bookings.FindAsync(booking.Id);
            Assert.NotNull(updatedBooking);
            Assert.Equal(BookingStatusEnum.Confirmed, updatedBooking.Status);
        }

        [Fact]
        public async Task DeleteAsync_RemovesBooking()
        {
            var eventId = await CreateTestEventAsync();
            var booking = new BookingModel(eventId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(booking);

            var deletedBooking = await _context.Bookings.FindAsync(booking.Id);
            Assert.Null(deletedBooking);
        }
    }
}
