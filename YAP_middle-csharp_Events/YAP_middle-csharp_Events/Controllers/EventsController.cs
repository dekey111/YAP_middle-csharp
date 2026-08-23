using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Events.Application.Interfaces.IServices;
using YAP_middle_csharp_Events.Application.Models;
using YAP_middle_csharp_Events.Domain.Exceptions;

namespace YAP_middle_csharp_Events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsController(IEventService eventService, ILogger<EventsController> logger) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly ILogger<EventsController> _logger = logger;

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
        [ProducesResponseType(typeof(PaginatedResult<EventContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllEventsAsync(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery, Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть не менее 1")] int page = 1,
            [FromQuery, Range(1, 200, ErrorMessage = "Размер страницы должен быть от 1 до 200")] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("[EventsController] [GetAllEvents]");

            var result = await _eventService.FindAllAsync(title, from, to, page, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий</param>
        /// <returns>Возвращает статус 200 и найденный элемент, либо 404 с комментарием</returns>
        [HttpGet("{id:Guid}")]
        [ProducesResponseType(typeof(EventContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("[EventsController] [GetEventById] Запрос на поиск EventId: {EventId}", id);

            var eventContract = await _eventService.FindByIdAsync(id, cancellationToken);
            if (eventContract == null)
            {
                _logger.LogDebug("[EventsController] [GetEventById] Event c id: {EventId} не найден!", id);
                throw new NotFoundExceptionApp($"Event с id: {id} не найден!");
            }

            return Ok(eventContract);
        }


        /// <summary>
        /// Топ-10 самых популярных событий
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("top")]
        [ProducesResponseType(typeof(IReadOnlyList<EventContract>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTop10EventsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("[EventsController] [GetTop10Events] Запрос на получение топ-10 событий");
            var result = await _eventService.FindTop10EventsAsync(cancellationToken);
            return Ok(result);
        }


        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="eventModel">Принимает модель события</param>
        /// <returns>Возвращает 201 с ссылкой на созданное событие</returns>
        /// <returns>Возвращает 400 в случае ошибки валидации события</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EventContract), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddEventAsync([FromBody] EventRequest eventRequest, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("[EventsController] [AddEvent] Запрос на добавление нового события");

            var createdEvent = await _eventService.CreateAsync(eventRequest, cancellationToken);
            return CreatedAtAction(nameof(GetEventByIdAsync), new { id = createdEvent.Id }, createdEvent);
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
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EventUpdateRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditEventAsync([FromRoute] Guid id, [FromBody] EventUpdateRequest eventUpdateRequest, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EventsController] [EditEvent] Запрос на изменения события {EventId}", id);
            var updatedEvent = await _eventService.UpdateAsync(id, eventUpdateRequest, cancellationToken);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <returns>возвращает - 204 в случае успеха</returns>
        /// <returns>Возвращает - 404 В случае если событие не найдено</returns>
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EventUpdateRequest), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEventAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EventsController] [DeleteEvent] Запрос на удаление события {EventId}", id);

            await _eventService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
