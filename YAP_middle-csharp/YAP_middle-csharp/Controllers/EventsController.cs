using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;


namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsController(IEventService eventService,
        IBookingService bookingService,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly IBookingService _bookingService = bookingService;
        private readonly ILogger<BookingController> _logger = logger;

        /// <summary>
        /// Метод получения всех событий
        /// </summary>
        /// <param name="title">Опциональное поле фильтрации по наименованию</param>
        /// <param name="from">Опциональное поле фильтрации по не ранее даты</param>
        /// <param name="to">Опциональное поле фильтрации по не позднее даты</param>
        /// <param name="page">Опциональное поле для выбора страницы, со значением по умолчанию = 1 </param>
        /// <param name="pageSize">Опциональное поле для выбора количества выгружаемых строк, со значением по умолчанию = 10</param>
        /// <returns>Возвращается Json-Структуру и статусом 200-OK в случае успеха</returns>
        /// <returns>Возвращает 400 в случае ошибки получения страниц или количество элементов на странице</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllEventsAsync(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery, Range(1, int.MaxValue, ErrorMessage ="Номер страницы должен быть не менее 1")] int page = 1,
            [FromQuery, Range(1, 200, ErrorMessage = "Размер страницы должен быть от 1 до 200")] int pageSize = 10)
        {
            _logger.LogDebug("[EventsController] [GetAllEvents]");

            var result = await _eventService.FindAllAsync(title, from, to, page, pageSize);
            var respondedItems = result.Items.Select(e => new EventResponse(e));

            return Ok(new PaginatedResult<EventResponse>
            {
                Items = respondedItems,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий</param>
        /// <returns>Возвращает статус 200 и найденный элемент, либо 404 с комментарием</returns>
        [HttpGet("{id:Guid}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventByIdAsync([FromRoute] Guid id)
        {
            _logger.LogDebug("[EventsController] [GetEventById] Запрос на поиск EventId: {EventId}", id);

            var findEvent = await _eventService.FindByIdAsync(id);
            if (findEvent == null)
            {
                _logger.LogDebug("[EventsController] [GetEventById] Event c id: {EventId} не найден!", id);
                throw new KeyNotFoundException($"Event c id: {id} не найден!");
            }

            return Ok(new EventResponse(findEvent));
        }

        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="eventModel">Принимает модель события</param>
        /// <returns>Возвращает 201 с ссылкой на созданное событие</returns>
        /// <returns>Возвращает 400 в случае ошибки валидации события</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddEventAsync([FromBody] EventRequest eventRequest)
        {
            _logger.LogDebug("[EventsController] [AddEvent] Запрос на добавление нового события");

            var createdEvent = await _eventService.CreateAsync(eventRequest);
            var newEventResponse = new EventResponse(createdEvent);

            return CreatedAtAction(nameof(GetEventByIdAsync), new { id = createdEvent.Id }, newEventResponse);
        }

        /// <summary>
        /// Создание нового бронирования на событие
        /// </summary>
        /// <param name="eventId">Принимает уникальный идентификатор события</param>
        /// <returns>202 - Возвращает новую бронь</returns>
        /// <returns>400 - Возвращается при ошибки валидации</returns>
        /// <returns>404 - Возвращается, в случае если не удалось найти событие </returns>
        /// <returns>409 - Возвращается если свободных мест больше нет</returns>
        /// <exception cref="NotFoundExceptionApp"></exception>
        [HttpPost("{eventId:guid}/book")]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddBookingByEventIdAsync([FromRoute] Guid eventId, [FromBody] Guid userId)
        {
            _logger.LogInformation("[EventsController] [AddBookingByEventId] Запрос на бронирование события {EventId}", eventId);
            var newBooking = await _bookingService.CreateBookingAsync(eventId, userId);
            var bookingResponse = new
            {
                id = newBooking.Id,
                eventId = newBooking.EventId,
                status = newBooking.Status.ToString(),
                createdAt = newBooking.CreatedAt,
                processedAt = newBooking.ProcessedAt,
                userId = newBooking.UserId
            };

            return AcceptedAtAction("GetBookingAsync", "Booking", new { id = newBooking.Id }, bookingResponse);
        }


        /// <summary>
        /// Отмена бронирования на событие
        /// </summary>
        /// <param name="eventId">Принимает уникальный идентификатор события</param>
        /// <param name="bookingId">Принимает уникальный идентификатор брони</param>
        /// <returns>204 - Успешная отмена брони, ничего не возвращает</returns>
        /// <returns>400 - Возвращается при ошибки валидации</returns>
        /// <returns>404 - Возвращается, в случае если не удалось найти событие </returns>
        /// <returns>409 - Возвращается если свободных мест больше нет</returns>
        /// <exception cref="NotFoundExceptionApp"></exception>
        [HttpPost("{eventId:guid}/book/{bookingId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelBookingAsync([FromRoute] Guid eventId, [FromRoute] Guid bookingId)
        {
            _logger.LogInformation("[EventsController] [CancelBookingAsync] Запрос на отмену брони {BookingId} для события {EventId}", bookingId, eventId);
            await _bookingService.CancelledBookingAsync(eventId, bookingId);
            return NoContent();
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <param name="eventModel">Принимает новую модель события из body</param>
        /// <returns>Возвращает - 200 OK c изменённым элементом, либо 400 с описанием ошибки</returns>
        /// <returns>Возвращает - 400 В случае ошибки валидации</returns>
        /// <returns>Возвращает - 404 В случае если событие не найдено</returns>
        [HttpPut("{id:Guid}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditEventAsync([FromRoute] Guid id, [FromBody] EventResponse eventResponse)
        {
            _logger.LogInformation("[EventsController] [EditEvent] Запрос на изменения события {EventId}", id);

            if (id != eventResponse.Id)
            {
                throw new ValidationExceptionApp("Проблема в сущности и в запросе. Проверьте правильность данных!");
            }

            var updatedEvent = await _eventService.UpdateAsync(eventResponse);
            return Ok(new EventResponse(updatedEvent));
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <returns>возвращает - 204 в случае успеха</returns>
        /// <returns>Возвращает - 404 В случае если событие не найдено</returns>
        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEventAsync([FromRoute] Guid id)
        {
            _logger.LogInformation("[EventsController] [DeleteEvent] Запрос на удаление события {EventId}", id);

            await _eventService.DeleteAsync(id);
            return NoContent();
        }
    }
}
