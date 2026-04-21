using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(IEventService eventService, IValidator<EventResponse> validator) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly IValidator<EventResponse> _validator = validator;

        /// <summary>
        /// Метод получения всех событий 
        /// </summary>
        /// <returns>Возвращается Json-Структура и статусом 200-OK в случае успеха</returns>
        [HttpGet]
        public IActionResult GetAllEvents(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return Ok(_eventService.FindAll(title, from, to, page, pageSize));
        }

        /// <summary>
        /// Метод получения конкретного события по id
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий</param>
        /// <returns>Возвращает статус 200 и найденный элемент, либо 404 с комментарием</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById([FromRoute] int id)
        {
            var findEvent = await _eventService.FindById(id);
            if (findEvent == null)
                return NotFound($"Event c id: {id} не найден!");

            return Ok(findEvent);
        }

        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="eventModel">Принимает модель события</param>
        /// <returns>Возвращает 201 с ссылкой на созданное событие</returns>
        [HttpPost]
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
        public async Task<IActionResult> EditEvent([FromRoute] int id, [FromBody] EventResponse eventModel)
        {
            if (id != eventModel.id)
                return BadRequest("Проблема в сущности и в запросе. \nПроверьте правильность данных!");

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
        public async Task<IActionResult> DeleteEvent([FromRoute] int id)
        {
            var findEvent = await _eventService.FindById(id);

            if (findEvent == null)
                return NotFound($"Event c id: {id} не найден!");

            await _eventService.Delete(findEvent);
            return NoContent();
        }
    }
}
