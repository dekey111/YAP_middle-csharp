using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Booking.Application.Interfaces;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Application.Services;
using YAP_middle_csharp_Booking.Domain.Exceptions;
using YAP_middle_csharp_Booking.Domain.Models;
using YAP_middle_csharp_Booking.Infrastructure.DataAccess;
using YAP_middle_csharp_Booking.Infrastructure.Repository;
using YAP_middle_csharp_Booking.Infrastructure.Services.BackgroundServices;
using UserRoleEnum = YAP_middle_csharp_Booking.Domain.Models.UserRoleEnum;

namespace YAP_middle_csharp_Booking.FuncTest
{
    public class BookingServiceTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IBookingService _bookingService;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddLogging();

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IUserContextService, UserContextService>();

            _serviceProvider = services.BuildServiceProvider();

            var scope = _serviceProvider.CreateScope();
            _bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        #region Успешные сценарии

        [Fact]
        public async Task Create_ReturnNewBooking()
        {
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = await _bookingService.CreateBookingAsync(eventId, userId);

            Assert.NotNull(booking);
            Assert.Equal(eventId, booking.EventId);
            Assert.Equal(userId, booking.UserId);
            Assert.Equal(BookingStatusEnum.Pending, booking.Status);
            Assert.NotEqual(Guid.Empty, booking.Id);
        }

        [Fact]
        public async Task CreateByOneEvent_ReturnUniqBooking()
        {
            var eventId = Guid.NewGuid();

            var booking1 = await _bookingService.CreateBookingAsync(eventId, Guid.NewGuid());
            var booking2 = await _bookingService.CreateBookingAsync(eventId, Guid.NewGuid());
            var booking3 = await _bookingService.CreateBookingAsync(eventId, Guid.NewGuid());

            Assert.NotNull(booking1);
            Assert.NotNull(booking2);
            Assert.NotNull(booking3);
            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);
        }

        [Fact]
        public async Task FindByIdForUserAsync_ReturnCurrentInfoByIdBooking_ForOwner()
        {
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var newBooking = await _bookingService.CreateBookingAsync(eventId, userId);

            var findBooking = await _bookingService.FindByIdForUserAsync(newBooking.Id, userId, UserRoleEnum.User);

            Assert.NotNull(findBooking);
            Assert.Equal(newBooking.Id, findBooking.Id);
            Assert.Equal(userId, findBooking.UserId);
        }

        [Fact]
        public async Task FindByIdForUserAsync_ReturnCurrentInfoByIdBooking_ForAdmin()
        {
            var eventId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            var newBooking = await _bookingService.CreateBookingAsync(eventId, ownerId);

            var findBooking = await _bookingService.FindByIdForUserAsync(newBooking.Id, adminId, UserRoleEnum.Admin);

            Assert.NotNull(findBooking);
            Assert.Equal(newBooking.Id, findBooking.Id);
        }

        [Fact]
        public async Task CancelledBookingAsyncByOwner_Success()
        {
            var ownerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = await _bookingService.CreateBookingAsync(eventId, ownerId);

            await _bookingService.CancelledBookingAsync(eventId, booking.Id, ownerId, UserRoleEnum.User);

            var cancelledBooking = await _bookingService.FindByIdForUserAsync(booking.Id, ownerId, UserRoleEnum.User);
            Assert.NotNull(cancelledBooking);
            Assert.Equal(BookingStatusEnum.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task CancelledBookingAsyncByAdmin_Success()
        {
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = await _bookingService.CreateBookingAsync(eventId, ownerId);

            await _bookingService.CancelledBookingAsync(eventId, booking.Id, adminId, UserRoleEnum.Admin);

            var cancelledBooking = await _bookingService.FindByIdForUserAsync(booking.Id, adminId, UserRoleEnum.Admin);
            Assert.NotNull(cancelledBooking);
            Assert.Equal(BookingStatusEnum.Cancelled, cancelledBooking.Status);
        }

        #endregion

        #region Неуспешные сценарии

        [Fact]
        public async Task FindByIdForUserAsync_ThrowsUnauthorizedOperationException_WhenStrangerRequests()
        {
            var eventId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();

            var booking = await _bookingService.CreateBookingAsync(eventId, ownerId);

            var ex = await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.FindByIdForUserAsync(booking.Id, strangerId, UserRoleEnum.User));
            Assert.Equal("Ошибка получения бронирования", ex.Message);
        }

        [Fact]
        public async Task CancelledBookingAsync_ThrowsUnauthorizedOperationException_WhenStrangerCancels()
        {
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = await _bookingService.CreateBookingAsync(eventId, ownerId);

            await Assert.ThrowsAsync<UnauthorizedOperationException>(() =>
                _bookingService.CancelledBookingAsync(eventId, booking.Id, strangerId, UserRoleEnum.User));
        }

        #endregion

        #region Тесты UserContextService

        [Fact]
        public void UserContextService_GetCurrentUserId_ReturnsCorrectGuid()
        {
            var userContext = new UserContextService();
            var expectedUserId = Guid.NewGuid();

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var result = userContext.GetCurrentUserId(principal);

            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public void UserContextService_GetCurrentUserRole_ReturnsCorrectRole()
        {
            var userContext = new UserContextService();

            var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var result = userContext.GetCurrentUserRole(principal);

            Assert.Equal(UserRoleEnum.Admin, result);
        }

        #endregion
    }
}
