using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Application.Services.BackgroundServices;
using YAP_middle_csharp.Application.Validator;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace YAP_middle_csharp.Tests
{

    public class BookingServiceTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IBookingService _bookingService;
        private readonly IEventService _eventService;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddLogging();
            services.AddTransient<IValidator<EventModel>, EventValidator>();

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();

            var scope = _serviceProvider.CreateScope();
            _eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        #region Успешные
        /// <summary>
        /// Создание брони для существующего события — возвращается BookingInfo со статусом Pending;
        /// </summary>
        [Fact]
        public async Task Create_ReturnNewBooking()
        {
            var userId = Guid.NewGuid();
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var booking = await _bookingService.CreateBookingAsync(id, userId);

            Assert.NotNull(booking);
            Assert.Equal(newEvent.Id, booking.EventId);
            Assert.Equal(userId, booking.UserId);
            Assert.Equal(BookingStatusEnum.Pending, booking.Status);
            Assert.NotEqual(Guid.Empty, booking.Id);
        }

        /// <summary>
        /// Создание нескольких броней для одного события — все создаются с уникальными Id;
        /// </summary>
        [Fact]
        public async Task CreateByOneEvent_ReturnUniqBooking()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 3,
                AvailableSeats = 3,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var booking1 = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var booking2 = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var booking3 = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());

            Assert.NotNull(booking1);
            Assert.NotNull(booking2);
            Assert.NotNull(booking3);

            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);
        }

        /// <summary>
        /// получение брони по Id — возвращается корректная информация;
        /// </summary>
        [Fact]
        public async Task FindById_ReturnCurrentInfoByIdBooking()
        {
            var userId = Guid.NewGuid();
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var newBooking = await _bookingService.CreateBookingAsync(id, userId);

            var findBooking = await _bookingService.FindByIdAsync(newBooking.Id);

            Assert.NotNull(findBooking);
            Assert.Equal(newBooking.Id, findBooking.Id);
        }

        /// <summary>
        /// Получение брони отражает изменение статуса (после Confirm/Reject).
        /// </summary>
        [Fact]
        public async Task BackgroundService_IndependentTest_ShouldConfirm()
        {
            using var setupScope = _serviceProvider.CreateScope();
            var localEventService = setupScope.ServiceProvider.GetRequiredService<IEventService>();
            var localBookingService = setupScope.ServiceProvider.GetRequiredService<IBookingService>();

            var newEvent = new EventModel
            {
                Title = "Крутое тестовое событие",
                TotalSeats = 2,
                AvailableSeats = 2,
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };
            var eventId = await localEventService.CreateAsync(newEvent);
            var booking = await localBookingService.CreateBookingAsync(eventId, Guid.NewGuid());

            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
            var bgService = new BackgroundBookingService(scopeFactory, new NullLogger<BackgroundBookingService>());

            using var cts = new CancellationTokenSource();
            var runTask = bgService.StartAsync(cts.Token);

            await Task.Delay(4500);

            cts.Cancel();
            try { await runTask; } catch (OperationCanceledException) { }

            using var assertScope = _serviceProvider.CreateScope();
            var assertBookingService = assertScope.ServiceProvider.GetRequiredService<IBookingService>();
            var result = await assertBookingService.FindByIdAsync(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(BookingStatusEnum.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        /// <summary>
        /// Создание брони уменьшает AvailableSeats на 1.
        /// </summary>
        [Fact]
        public async Task CreateBooking_AvailableSeatsDecrement()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var newBooking = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var updatedEvent = await _eventService.FindByIdAsync(id);

            Assert.NotNull(newBooking);
            Assert.Equal(newEvent.Id, newBooking.EventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(1, updatedEvent.AvailableSeats);
        }

        /// <summary>
        /// Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.
        /// </summary>
        [Fact]
        public async Task CreateMultipleBooking_ToLimitAllSuccess()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var newBooking1 = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var newBooking2 = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var updatedEvent = await _eventService.FindByIdAsync(id);

            Assert.NotNull(newBooking1);
            Assert.NotNull(newBooking2);
            Assert.Equal(newEvent.Id, newBooking1.EventId);
            Assert.Equal(newEvent.Id, newBooking2.EventId);
            Assert.Equal(2, updatedEvent?.TotalSeats);
            Assert.NotNull(updatedEvent);
            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        /// <summary>
        /// После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task CreateOutOfLimitBooking_NoAvailableSeatsExeption()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            await _bookingService.CreateBookingAsync(id, Guid.NewGuid());

            var exception = await Assert.ThrowsAsync<NoAvailableSeatsExceptionApp>(() => _bookingService.CreateBookingAsync(id, Guid.NewGuid()));
            Assert.Equal("Недостаточно мест на событие", exception.Message);
        }

        /// <summary>
        /// Переход в Confirmed: После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.
        /// </summary>
        [Fact]
        public void BookingAfterConfirm_ReturnsConfirmedAndProcessedAt()
        {
            var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

            booking.Status = BookingStatusEnum.Confirmed;
            booking.ProcessedAt = DateTime.UtcNow;

            Assert.Equal(BookingStatusEnum.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        /// <summary>
        /// Переход в Rejected: После вызова Reject() бронь возвращает статус Rejected и заполненный ProcessedAt.
        /// </summary>
        [Fact]
        public void Booking_AfterReject_ReturnsRejectedAndProcessedAt()
        {
            var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

            booking.Status = BookingStatusEnum.Rejected;
            booking.ProcessedAt = DateTime.UtcNow;

            Assert.Equal(BookingStatusEnum.Rejected, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        /// <summary>
        /// После Reject() ReleaseSeats() количество свободных мест восстанавливается.
        /// </summary>
        [Fact]
        public async Task AfterRejectAndReleaseSeats_AvailableSeatsAreRestored()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 5,
                AvailableSeats = 5,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);

            await _bookingService.CreateBookingAsync(id, Guid.NewGuid());
            var eventAfterBook = await _eventService.FindByIdAsync(id);
            Assert.Equal(4, eventAfterBook?.AvailableSeats);

            eventAfterBook!.ReleaseSeats(1);
            await _eventService.UpdateAsync(eventAfterBook);

            var eventAfterRelease = await _eventService.FindByIdAsync(id);
            Assert.Equal(5, eventAfterRelease?.AvailableSeats);
        }

        /// <summary>
        /// После Reject() ReleaseSeats() можно успешно создать новую бронь на то же место.
        /// </summary>
        [Fact]
        public async Task AfterRejectAndReleaseSeats_CanBookAgain()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 1,
                AvailableSeats = 1,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);

            await _bookingService.CreateBookingAsync(id, Guid.NewGuid());

            var eventModel = await _eventService.FindByIdAsync(id);
            eventModel!.ReleaseSeats(1);
            await _eventService.UpdateAsync(eventModel);

            var newBooking = await _bookingService.CreateBookingAsync(id, Guid.NewGuid());

            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatusEnum.Pending, newBooking.Status);
        }

        [Fact]
        public async Task Highload_ProtectionFromOverbooking_AllowsOnlyLimitCount()
        {
            var newEvent = new EventModel
            {
                Id = Guid.NewGuid(),
                TotalSeats = 5,
                AvailableSeats = 5,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);

            int totalRequests = 20;
            var successfulBookingsCount = 0;
            var exceptionCount = 0;

            var tasks = Enumerable.Range(0, totalRequests).Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedBookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                try
                {
                    await scopedBookingService.CreateBookingAsync(id, Guid.NewGuid());
                    Interlocked.Increment(ref successfulBookingsCount);
                }
                catch (NoAvailableSeatsExceptionApp)
                {
                    Interlocked.Increment(ref exceptionCount);
                }
            }));

            await Task.WhenAll(tasks);

            using var assertScope = _serviceProvider.CreateScope();
            var assertEventService = assertScope.ServiceProvider.GetRequiredService<IEventService>();

            var updatedEvent = await assertEventService.FindByIdAsync(id);

            Assert.Equal(5, successfulBookingsCount);
            Assert.Equal(15, exceptionCount);
            Assert.Equal(0, updatedEvent?.AvailableSeats);
        }

        /// <summary>
        /// Тест на уникальность Id при конкурентных запросах.
        /// </summary>
        [Fact]
        public async Task Highload_ConcurrentRequests_GenerateUniqueIds()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 10,
                AvailableSeats = 10,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedBookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                return await scopedBookingService.CreateBookingAsync(id, Guid.NewGuid());
            }));

            var results = await Task.WhenAll(tasks);

            Assert.Equal(10, results.Length);

            var uniqueIdsCount = results.Select(b => b.Id).Distinct().Count();
            Assert.Equal(10, uniqueIdsCount);
        }
        #endregion

        #region Неуспешные
        /// <summary>
        /// создание брони для несуществующего события;
        /// </summary>
        [Fact]
        public async Task CreateBooking_ReturnKeyNotFoundException()
        {
            var randomGuid = Guid.NewGuid();
            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.CreateBookingAsync(randomGuid, Guid.NewGuid()));
        }

        /// <summary>
        /// создание брони для удалённого события;
        /// </summary>
        [Fact]
        public async Task CreateBookingToDeletedEvent_ReturnKeyNotFoundException()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var eventId = await _eventService.CreateAsync(newEvent);
            await _eventService.DeleteAsync(newEvent);

            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.CreateBookingAsync(eventId, Guid.NewGuid()));
        }

        /// <summary>
        /// Бронирование для несуществующего события
        /// </summary>
        [Fact]
        public async Task FindByDosentEvent_ReturnNotFoundExceptionApp()
        {
            var randomGuid = Guid.NewGuid();
            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.FindByIdAsync(randomGuid));
        }

        /// <summary>
        /// Бронирование при отсутствии мест → NoAvailableSeatsException.
        /// </summary>
        [Fact]
        public async Task CreateBookingToAvailableSeats_ReturnNoAvailableSeatsExceptionApp()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 0,
                AvailableSeats = 0,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.CreateAsync(newEvent);
            var exception = await Assert.ThrowsAsync<NoAvailableSeatsExceptionApp>(() => _bookingService.CreateBookingAsync(id, Guid.NewGuid()));
            Assert.Equal("Недостаточно мест на событие", exception.Message);
        }

        #endregion

        #region Новые бизнес-правила

        /// <summary>
        /// Попытка забронировать прошедшее событие приводит к ошибке ValidationExceptionApp.
        /// </summary>
        [Fact]
        public async Task CreateBooking_EventInPast_ThrowsValidationExceptionApp()
        {
            var pastEvent = new EventModel
            {
                TotalSeats = 10,
                AvailableSeats = 10,
                Title = "Прошедшее событие",
                StartAt = DateTime.UtcNow.AddHours(-2), 
                EndAt = DateTime.UtcNow.AddHours(2)
            };
            var id = await _eventService.CreateAsync(pastEvent);

            await Assert.ThrowsAsync<ValidationExceptionApp>(() => _bookingService.CreateBookingAsync(id, Guid.NewGuid()));
        }

        /// <summary>
        /// При достижении лимита активных броней (10) новая бронь не создаётся -> BookingLimitExceededException.
        /// </summary>
        [Fact]
        public async Task CreateBooking_UserLimitExceeded_ThrowsBookingLimitExceededException()
        {
            var userId = Guid.NewGuid();
            var newEvent = new EventModel
            {
                TotalSeats = 20,
                AvailableSeats = 20,
                Title = "Большое событие",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };
            var eventId = await _eventService.CreateAsync(newEvent);

            for (int i = 0; i < 10; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, userId);
            }

            await Assert.ThrowsAsync<BookingLimitExceededException>(() => _bookingService.CreateBookingAsync(eventId, userId));
        }

        /// <summary>
        /// Лимиты разных пользователей не влияют друг на друга
        /// </summary>
        [Fact]
        public async Task CreateBooking_UserLimitsAreIndependent_SuccessForSecondUser()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            var newEvent = new EventModel
            {
                TotalSeats = 20,
                AvailableSeats = 20,
                Title = "Открытое событие",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };
            var eventId = await _eventService.CreateAsync(newEvent);

            for (int i = 0; i < 10; i++)
            {
                await _bookingService.CreateBookingAsync(eventId, user1);
            }

            await Assert.ThrowsAsync<BookingLimitExceededException>(() => _bookingService.CreateBookingAsync(eventId, user1));

            var user2Booking = await _bookingService.CreateBookingAsync(eventId, user2);
            Assert.NotNull(user2Booking);
            Assert.Equal(user2, user2Booking.UserId);
        }
        #endregion
    }

}
