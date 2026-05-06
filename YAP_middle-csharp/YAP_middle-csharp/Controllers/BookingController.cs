using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Services;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BookingController(
        IBookingServive bookingService,
        IValidator<BookingModel> validator,
        ILogger<BookingController> logger,
        IEventService eventService) : ControllerBase
    {
        private readonly IBookingServive _bookingService = bookingService;
        private readonly IValidator<BookingModel> _validator = validator;
        private readonly ILogger<BookingController> _logger = logger;
        private readonly IEventService _eventService = eventService;

        /// <summary>
        /// Создание нового бронирования на событие
        /// </summary>
        /// <param name="eventId">Принимает уникальный идентификатор события</param>
        /// <returns>Возвращает новую бронь</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpPost("/events/{eventId:guid}/book")]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBooking(Guid eventId)
        {
            _logger.LogInformation("[BookingController] Запрос на бронирование события {EventId}", eventId);

            var findEvent = await _eventService.FindById(eventId);
            if (findEvent == null)
            {
                _logger.LogWarning("[BookingController] Событие {EventId} не найдено для бронирования", eventId);
                throw new KeyNotFoundException($"Событие не найдено");
            }
            var newBooking = await _bookingService.CreateBookingAsync(eventId);
            var locationUrl = $"/bookings/{newBooking.Id}";

            return Accepted(locationUrl, newBooking);
        }

        [HttpGet("/bookings/{id:guid}")]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            _logger.LogInformation("[BookingController] Запрос данных брони {BookingId}", id);

            var booking = await _bookingService.FindById(id);

            if (booking == null)
            {
                _logger.LogWarning("[BookingController] Бронь {BookingId} не найдена", id);
                throw new KeyNotFoundException($"Бронь не найдена");
            }
            return Ok(booking);
        }
    }
}
