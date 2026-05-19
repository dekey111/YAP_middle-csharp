using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
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
            var eventSerive = new EventService(eventRepository, new NullLogger<EventService>());

            _bookingService = new BookingService(bookingRepository, bookingLogger, eventSerive);
            _eventService = new EventService(eventRepository, eventLogger);
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
            var eventService = new EventService(eventRepository, new NullLogger<EventService>());

            var localBookingService = new BookingService(bookingRepository, new NullLogger<BookingService>(), eventService);

            var newEvent = new EventModel
            {
                Title = "Тестовое событие",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };
            var eventId = await eventService.Create(newEvent);

            var booking = await localBookingService.CreateBookingAsync(eventId);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceProviderMock.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IBookingRepository))).Returns(bookingRepository);

            var bgService = new BackgroundBookingService(serviceProviderMock.Object, new NullLogger<BackgroundBookingService>());

            using var cts = new CancellationTokenSource();
            var runTask = bgService.StartAsync(cts.Token);

            await Task.Delay(3000);

            cts.Cancel();
            try { await runTask; } catch (OperationCanceledException) { }

            var result = await localBookingService.FindById(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(BookingStatusEnum.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(randomGuid));
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
                Title = "Какой то суперский Ивент",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddMonths(1)
            };
            var eventId = await _eventService.Create(newEvent);
            await _eventService.Delete(newEvent);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        }


        /// <summary>
        /// Получение брони по несуществующему Id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FindByDosentEvent_ReturnKeyNotFoundException()
        {
            var randomGuid = Guid.NewGuid();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.FindById(randomGuid));
        }
        #endregion

    }
}
