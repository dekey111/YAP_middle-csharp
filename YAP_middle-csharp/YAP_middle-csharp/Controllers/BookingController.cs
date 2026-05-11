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
        IBookingService bookingService,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;
        private readonly ILogger<BookingController> _logger = logger;

        /// <summary>
        /// Метод получения брони
        /// </summary>
        /// <param name="id">Принимает Уникальный идентификатор брони</param>
        /// <returns>Возвращает бронь</returns>
        /// <exception cref="NotFoundExceptionApp"></exception>
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
