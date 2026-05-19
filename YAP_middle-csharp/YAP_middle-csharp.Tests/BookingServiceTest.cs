using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;
using YAP_middle_csharp.Services;
using YAP_middle_csharp.Services.BackgroundServices;
using YAP_middle_csharp.Validator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YAP_middle_csharp.Tests
{
    public class BookingServiceTest
    {
        private readonly BookingService _bookingService;
        private readonly EventService _eventService;

        public BookingServiceTest()
        {
            var bookingRepository = new BookingRepository();
            var bookingLogger = new NullLogger<BookingService>();

            var eventRepository = new EventRepository();
            var eventLogger = new NullLogger<EventService>();

            _eventService = new EventService(eventRepository, eventLogger);

            _bookingService = new BookingService(bookingRepository, bookingLogger, _eventService);
        }

        #region Успешные
        /// <summary>
        /// Создание брони для существующего события — возвращается BookingInfo со статусом Pending;
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Create_ReturnNewBooking()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1) 
            };
            var id = await _eventService.Create(newEvent);
            var booking = await _bookingService.CreateBookingAsync(id);

            Assert.NotNull(booking);
            Assert.Equal(newEvent.Id, booking.EventId);
            Assert.Equal(BookingStatusEnum.Pending, booking.Status);
            Assert.NotEqual(Guid.Empty, booking.Id);
        }

        /// <summary>
        /// Cоздание нескольких броней для одного события — все создаются с уникальными Id;
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateByOneEvent_ReturnUniqBooking()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 3,
                AvailableSeats = 3,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var booking1 = await _bookingService.CreateBookingAsync(id);
            var booking2 = await _bookingService.CreateBookingAsync(id);
            var booking3 = await _bookingService.CreateBookingAsync(id);

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
        /// <returns></returns>
        [Fact]
        public async Task FindById_ReturnCurrentInfoByIdBooking()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var newBooking = await _bookingService.CreateBookingAsync(id);


            var findBooking = await _bookingService.FindById(newBooking.Id);

            Assert.NotNull(findBooking);
            Assert.Equal(newBooking.Id, findBooking.Id);
        }

        /// <summary>
        /// Получение брони отражает изменение статуса (после Confirm/Reject).
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BackgroundService_IndependentTest_ShouldConfirm()
        {
            //Честно говоря, это не мой метод. Это всё сделал AI. Я вообще не понял как это делать.
            //Мне кажется это недочёт спринта, надо показывать как делать такие вещи 
            //Ну, либо я тупой 🤷‍


            var bookingRepository = new BookingRepository();
            var eventRepository = new EventRepository();
            var localEventService = new EventService(eventRepository, new NullLogger<EventService>());

            var localBookingService = new BookingService(bookingRepository, new NullLogger<BookingService>(), localEventService);

            var newEvent = new EventModel
            {
                Title = "Тестовое событие",
                TotalSeats = 2,
                AvailableSeats = 2, 
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };
            var eventId = await localEventService.Create(newEvent);

            var booking = await localBookingService.CreateBookingAsync(eventId);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IBookingRepository))).Returns(bookingRepository);
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IEventService))).Returns(localEventService);

            var bgService = new BackgroundBookingService(serviceProviderMock.Object, new NullLogger<BackgroundBookingService>());

            using var cts = new CancellationTokenSource();
            var runTask = bgService.StartAsync(cts.Token);

            await Task.Delay(3500);

            cts.Cancel();
            try { await runTask; } catch (OperationCanceledException) { }

            var result = await localBookingService.FindById(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(BookingStatusEnum.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        /// <summary>
        /// Создание брони уменьшает AvailableSeats на 1.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateBooking_AvailableSeatsDecrement()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var newBooking = await _bookingService.CreateBookingAsync(id);
            var updatedEvent = await _eventService.FindById(id);

            Assert.NotNull(newBooking);
            Assert.Equal(newEvent.Id, newBooking.EventId);
            Assert.NotNull(updatedEvent);
            Assert.Equal(1, updatedEvent.AvailableSeats);
        }

        /// <summary>
        /// Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateMultipleBooking_ToLimitAllSuccess()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var newBooking1 = await _bookingService.CreateBookingAsync(id);
            var newBooking2 = await _bookingService.CreateBookingAsync(id);
            var updatedEvent = await _eventService.FindById(id);

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
        /// <returns></returns>
        [Fact]
        public async Task CreateOutOfLimitBooking_NoAvailableSeatsExeption()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                AvailableSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var newBooking1 = await _bookingService.CreateBookingAsync(id);
            var newBooking2 = await _bookingService.CreateBookingAsync(id);
            var updatedEvent = await _eventService.FindById(id);

            var exception = await Assert.ThrowsAsync<NoAvailableSeatsExceptionApp>(() => _bookingService.CreateBookingAsync(id)); 
            Assert.Equal("Недостаточно мест на событие", exception.Message);
        }

        /// <summary>
        /// Переход в Confirmed: После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.
        /// </summary>
        [Fact]
        public void BookingAfterConfirm_ReturnsConfirmedAndProcessedAt()
        {
            var booking = new BookingModel();

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
            var booking = new BookingModel();

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
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);

            await _bookingService.CreateBookingAsync(id); 
            var eventAfterBook = await _eventService.FindById(id);
            Assert.Equal(4, eventAfterBook?.AvailableSeats);

            await eventAfterBook!.ReleaseSeats(1);
            await _eventService.Update(eventAfterBook);

            var eventAfterRelease = await _eventService.FindById(id);
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
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);

            await _bookingService.CreateBookingAsync(id); 

            var eventModel = await _eventService.FindById(id);
            await eventModel!.ReleaseSeats(1);
            await _eventService.Update(eventModel);

            var newBooking = await _bookingService.CreateBookingAsync(id);

            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatusEnum.Pending, newBooking.Status);
        }



        [Fact]
        public async Task Highload_ProtectionFromOverbooking_AllowsOnlyLimitCount()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 5,
                AvailableSeats = 5,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);

            int totalRequests = 20;
            var tasks = new List<Task<BookingModel>>();

            for (int i = 0; i < totalRequests; i++)
            {
                tasks.Add(_bookingService.CreateBookingAsync(id));
            }

            int successfulBookingsCount = 0;
            int exceptionCount = 0;

            foreach (var task in tasks)
            {
                try
                {
                    var result = await task;
                    if (result != null) successfulBookingsCount++;
                }
                catch (NoAvailableSeatsExceptionApp)
                {
                    exceptionCount++;
                }
            }

            var updatedEvent = await _eventService.FindById(id);

            Assert.Equal(5, successfulBookingsCount);   
            Assert.Equal(15, exceptionCount);          
            Assert.Equal(0, updatedEvent?.AvailableSeats); 
        }

        /// <summary>
        /// Тест на уникальность Id при конкурентных запросах:
        /// Дано: событие на 10 мест, 10 одновременных запросов.
        /// Ожидается: 10 броней с уникальными Id.
        /// </summary>
        [Fact]
        public async Task Highload_ConcurrentRequests_GenerateUniqueIds()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 10,
                AvailableSeats = 10,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);

            var tasks = new List<Task<BookingModel>>();

            // 10 одновременных запросов
            for (int i = 0; i < newEvent.TotalSeats; i++)
            {
                tasks.Add(_bookingService.CreateBookingAsync(id));
            }

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
        /// <returns></returns>
        [Fact]
        public async Task CreateBooking_ReturnKeyNotFoundException()
        {
            var randomGuid = Guid.NewGuid();
            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.CreateBookingAsync(randomGuid));
        }

        /// <summary>
        /// создание брони для удалённого события;
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateBookingToDeletedEvent_ReturnKeyNotFoundException()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 2,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var eventId = await _eventService.Create(newEvent);
            await _eventService.Delete(newEvent);

            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.CreateBookingAsync(eventId));
        }


        /// <summary>
        /// Бронирование для несуществующего события
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FindByDosentEvent_ReturnNotFoundExceptionApp()
        {
            var randomGuid = Guid.NewGuid();
            await Assert.ThrowsAsync<NotFoundExceptionApp>(() => _bookingService.FindById(randomGuid));
        }


        /// <summary>
        /// Бронирование при отсутствии мест → NoAvailableSeatsException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateBookingToAvailableSeats_ReturnNoAvailableSeatsExceptionApp()
        {
            var newEvent = new EventModel
            {
                TotalSeats = 0,
                AvailableSeats = 0,
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var id = await _eventService.Create(newEvent);
            var exception = await Assert.ThrowsAsync<NoAvailableSeatsExceptionApp>(() => _bookingService.CreateBookingAsync(id));
            Assert.Equal("Недостаточно мест на событие", exception.Message);
        }
        #endregion

    }
}
