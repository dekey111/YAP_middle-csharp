using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Application.Models;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BookingsController(
       IUserContextService userContext,
       IBookingService bookingService,
       ILogger<BookingsController> logger) : ControllerBase
    {
        private readonly IUserContextService _userContext = userContext;
        private readonly IBookingService _bookingService = bookingService;
        private readonly ILogger<BookingsController> _logger = logger;

        /// <summary>
        /// Метод получения конкретной брони по её ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var idUserFromRequest = _userContext.GetCurrentUserId(User);
            var currentUserRole = _userContext.GetCurrentUserRole(User);

            _logger.LogInformation("[BookingsController] Запрос данных брони {BookingId}", id);

            var booking = await _bookingService.FindByIdForUserAsync(id, idUserFromRequest, currentUserRole, cancellationToken);
            return Ok(booking);
        }

        /// <summary>
        /// Создание нового бронирования на событие
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpPost("events/{eventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddBookingByEventIdAsync([FromBody] CreateBookingRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _userContext.GetCurrentUserId(User);

            _logger.LogInformation("[BookingsController] Запрос на бронирование события {EventId} пользователем {UserId}", request.EventId, userId);

            var newBooking = await _bookingService.CreateBookingAsync(request.EventId, userId, request.SeatsCount, cancellationToken);
            return AcceptedAtAction(nameof(GetBookingAsync), new { id = newBooking.Id }, newBooking);
        }

        /// <summary>
        /// Отмена бронирования
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpDelete("{bookingId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelBookingAsync([FromRoute] Guid bookingId, [FromQuery] Guid? eventId, CancellationToken cancellationToken = default)
        {
            var currentUserId = _userContext.GetCurrentUserId(User);
            var currentUserRole = _userContext.GetCurrentUserRole(User);

            _logger.LogInformation("[BookingsController] Запрос на отмену брони {BookingId}", bookingId);

            await _bookingService.CancelledBookingAsync(eventId ?? Guid.Empty, bookingId, currentUserId, currentUserRole, cancellationToken);
            return NoContent();
        }
    }
}
