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
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IBookingServive _bookingService = bookingService;
        private readonly ILogger<BookingController> _logger = logger;

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
            var newBooking = await _bookingService.CreateBookingAsync(eventId);
            return AcceptedAtAction(nameof(GetBooking), new { id = newBooking.Id }, newBooking);
        }

        /// <summary>
        /// Метод получения брони
        /// </summary>
        /// <param name="id">Принимает Уникальный идентификатор брони</param>
        /// <returns>Возвращает бронь</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("/bookings/{id:guid}")]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            _logger.LogInformation("[BookingController] Запрос данных брони {BookingId}", id);
            return Ok(await _bookingService.FindById(id));
        }
    }
}
