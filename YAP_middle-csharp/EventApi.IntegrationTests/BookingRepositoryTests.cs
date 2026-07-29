using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;


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

        private async Task<Guid> CreateTestUserAsync(string login = "testuser")
        {
            var user = new UserModel(login, "hash_password", UserRoleEnum.User);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

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
                EndAt = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();
            return eventId;
        }

        [Fact]
        public async Task StartQueryToFindAllAsync_ReturnsQueryable()
        {
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            var booking1 = new BookingModel(eventId, userId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var booking2 = new BookingModel(eventId, userId)
            {
                Status = BookingStatusEnum.Rejected,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.AddRange(booking1, booking2);
            await _context.SaveChangesAsync();

            var paginatedResult = await _repository.GetPagedAsync(null, null, null, 1, 10);

            Assert.Equal(2, paginatedResult.Items.Count());
            Assert.Equal(2, paginatedResult.TotalCount);
        }

        [Fact]
        public async Task FindPendingBookingsAsync_ReturnsOnlyPending()
        {
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            var pendingBooking = new BookingModel(eventId, userId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var confirmedBooking = new BookingModel(eventId, userId)
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
            var userId = await CreateTestUserAsync();

            var booking = new BookingModel(eventId, userId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _repository.FindByIdAsync(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(booking.Id, result.Id);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task CreateAsync_SavesBookingToDatabase()
        {
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            var booking = new BookingModel(eventId, userId)
            {
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(booking);

            var savedBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
            Assert.NotNull(savedBooking);
            Assert.Equal(eventId, savedBooking.EventId);
            Assert.Equal(userId, savedBooking.UserId);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingBooking()
        {
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            var booking = new BookingModel(eventId, userId)
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
            var userId = await CreateTestUserAsync();

            var booking = new BookingModel(eventId, userId)
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

        [Fact]
        public async Task CheckActiveCountBookingByUserId_ReturnsCorrectCount()
        {
            var eventId = await CreateTestEventAsync();
            var userId1 = await CreateTestUserAsync("user1");
            var userId2 = await CreateTestUserAsync("user2");

            var b1 = new BookingModel(eventId, userId1) { Status = BookingStatusEnum.Pending };
            var b2 = new BookingModel(eventId, userId1) { Status = BookingStatusEnum.Confirmed };
            var b3 = new BookingModel(eventId, userId1) { Status = BookingStatusEnum.Cancelled };

            var b4 = new BookingModel(eventId, userId2) { Status = BookingStatusEnum.Pending };

            _context.Bookings.AddRange(b1, b2, b3, b4);
            await _context.SaveChangesAsync();

            var user1ActiveCount = await _repository.CheckActiveCountBookingByUserId(userId1);
            var user2ActiveCount = await _repository.CheckActiveCountBookingByUserId(userId2);

            Assert.Equal(2, user1ActiveCount);
            Assert.Equal(1, user2ActiveCount);
        }
    }
}
