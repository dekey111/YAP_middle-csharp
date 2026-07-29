using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BookingController(IUserContextService userContext,
        IBookingService bookingService,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IUserContextService _userContext = userContext;
        private readonly IBookingService _bookingService = bookingService;
        private readonly ILogger<BookingController> _logger = logger;

        /// <summary>
        /// Метод получения брони
        /// </summary>
        /// <param name="id">Принимает Уникальный идентификатор брони</param>
        /// <returns>Возвращает бронь</returns>
        /// <exception cref="NotFoundExceptionApp"></exception>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingAsync(Guid id)
        {
            var idUserFromRequest = _userContext.GetCurrentUserId(User);

            _logger.LogInformation("[BookingController] Запрос данных брони {BookingId}", id);
            return Ok(await _bookingService.FindByIdForUserAsync(id, idUserFromRequest));
        }
    }
}
