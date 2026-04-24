using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventsController(IEventService eventService, IValidator<EventResponse> validator) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly IValidator<EventResponse> _validator = validator;

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
            var result = await _eventService.FindAll(title, from, to, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий</param>
        /// <returns>Возвращает статус 200 и найденный элемент, либо 404 с комментарием</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById([FromRoute] int id)
        {
            var findEvent = await _eventService.FindById(id);
            if (findEvent == null)
               throw new KeyNotFoundException($"Event c id: {id} не найден!");

            return Ok(findEvent);
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
            var eventModel = new EventResponse
            {
                Title = eventRequest.Title,
                Description = eventRequest.Description,
                StartAt = eventRequest.StartAt,
                EndAt = eventRequest.EndAt
            };
            
            var errors = _validator.GetErrors(eventModel).ToList();
            if (errors.Any())
                throw new ValidationException(string.Join("; ", errors));

            int newIdEvent = await _eventService.Create(eventModel);
            return CreatedAtAction(nameof(GetEventById), new { id = newIdEvent }, eventModel);
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <param name="eventModel">Принимает новую модель события из body</param>
        /// <returns>Возвращает статус 200 OK c изменённым элементом, либо 400 с описанием ошибки</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditEvent([FromRoute] int id, [FromBody] EventResponse eventModel)
        {
            if (id != eventModel.Id)
                throw new ArgumentException("Проблема в сущности и в запросе. \nПроверьте правильность данных!");

            var errors = _validator.GetErrors(eventModel).ToList();
            if (errors.Any())
                throw new ValidationException(string.Join("; ", errors));

            var updatedEvent = await _eventService.Update(eventModel);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <returns>возвращает статус 204 в случае успеха, либо 400 с описанием ошибки</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent([FromRoute] int id)
        {
            var findEvent = await _eventService.FindById(id);

            if (findEvent == null)
                throw new KeyNotFoundException($"Event c id: {id} не найден!");

            await _eventService.Delete(findEvent);
            return NoContent();
        }
    }
}
