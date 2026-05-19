using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Exceptions;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Services;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsController(IEventService eventService,
        IValidator<EventModel> validator,
        IBookingService bookingService,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly IValidator<EventModel> _validator = validator;
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
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery, Range(1, int.MaxValue, ErrorMessage ="Номер страницы должен быть не менее 1")] int page = 1,
            [FromQuery, Range(1, 200, ErrorMessage = "Размер страницы должен быть от 1 до 200")] int pageSize = 10)
        {
            _logger.LogDebug("[EventsController] [GetAllEvents]");

            var result = await _eventService.FindAll(title, from, to, page, pageSize);
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
        public async Task<IActionResult> GetEventById([FromRoute] Guid id)
        {
            _logger.LogDebug("[EventsController] [GetEventById] Запрос на поиск EventId: {EventId}", id);

            var findEvent = await _eventService.FindById(id);
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
        [HttpPost]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddEvent([FromBody] EventRequest eventRequest)
        {
            _logger.LogDebug("[EventsController] [AddEvent] Запрос на добавление нового события");

            var eventModel = new EventModel()
            {
                Title = eventRequest.Title,
                Description = eventRequest.Description,
                TotalSeats = eventRequest.TotalSeats ?? 0,
                AvailableSeats = eventRequest.TotalSeats ?? 0,
                StartAt = eventRequest.StartAt,
                EndAt = eventRequest.EndAt
            };
            
            var errors = _validator.GetErrors(eventModel).ToList();
            if (errors.Any())
            {
                _logger.LogDebug(string.Join("; ", errors), "[EventsController] [AddEvent]");
                throw new ValidationExceptionApp(string.Join("; ", errors));
            }

            var newIdEvent = await _eventService.Create(eventModel);
            var newEvent = new EventResponse(eventModel);
            return CreatedAtAction(nameof(GetEventById), new { id = newIdEvent }, newEvent);
        }

        /// <summary>
        /// Создание нового бронирования на событие
        /// </summary>
        /// <param name="eventId">Принимает уникальный идентификатор события</param>
        /// <returns>Возвращает новую бронь</returns>
        /// <exception cref="NotFoundExceptionApp"></exception>
        [HttpPost("{eventId:guid}/book")]
        [ProducesResponseType(typeof(BookingModel), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBookingByEventId(Guid eventId)
        {
            _logger.LogInformation("[EventsController] [AddBookingByEventId] Запрос на бронирование события {EventId}", eventId);
            var newBooking = await _bookingService.CreateBookingAsync(eventId);
            return AcceptedAtAction("GetBooking", "Booking", new { id = newBooking.Id }, newBooking);
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <param name="eventModel">Принимает новую модель события из body</param>
        /// <returns>Возвращает статус 200 OK c изменённым элементом, либо 400 с описанием ошибки</returns>
        [HttpPut("{id:Guid}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditEvent([FromRoute] Guid id, [FromBody] EventResponse eventResponse)
        {
            _logger.LogInformation("[EventsController] [EditEvent] Запрос на изменения события {EventId}", id);

            if (id != eventResponse.Id)
            {
                _logger.LogWarning("[EventsController] [EditEvent] Проблема в сущности и в запросе. \nПроверьте правильность данных! {EventId}", id);
                throw new ValidationExceptionApp("Проблема в сущности и в запросе. \nПроверьте правильность данных!");
            }

            var eventModel = new EventModel()
            {
                Id = eventResponse.Id,
                Title = eventResponse.Title,
                Description = eventResponse.Description,
                TotalSeats = eventResponse.TotalSeats,
                AvailableSeats = eventResponse.AvailableSeats,
                StartAt = eventResponse.StartAt,
                EndAt = eventResponse.EndAt
            };

            var errors = _validator.GetErrors(eventModel).ToList();
            if (errors.Any())
            {
                _logger.LogInformation(string.Join("; ", errors), "[EventsController] [EditEvent] {EventId}", id);
                throw new ValidationExceptionApp(string.Join("; ", errors));
            }

            var updatedEvent = await _eventService.Update(eventModel);

            var returnUpdatedEvent = new EventResponse(updatedEvent);
            return Ok(returnUpdatedEvent);
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <returns>возвращает статус 204 в случае успеха, либо 400 с описанием ошибки</returns>
        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent([FromRoute] Guid id)
        {
            _logger.LogInformation("[EventsController] [DeleteEvent] Запрос на удаление события {EventId}", id);

            var findEvent = await _eventService.FindById(id);

            if (findEvent == null)
            {
                _logger.LogInformation("[EventsController] [DeleteEvent]  Event c id: {id} не найден!", id);
                throw new NotFoundExceptionApp($"Event c id: {id} не найден!");
            }

            await _eventService.Delete(findEvent);
            return NoContent();
        }
    }
}
