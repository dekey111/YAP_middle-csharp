using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Middleware;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EventsController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        /// <summary>
        /// Метод получения всех событий 
        /// </summary>
        /// <returns>Возвращается Json-Структура и статусом 200-OK в случае успеха</returns>
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            try
            {
                return Ok(_eventService.FindAll());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Меьтод получения конкретного события по id
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий</param>
        /// <returns>Возвращает статус 200 и найденный элемент, либо 404 с комментарием</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById([FromRoute] int id)
        {
            try
            {
                var findEvent = await _eventService.FindById(id);
                if (findEvent == null)
                    return NotFound($"Event c id: {id} не найден!");

                return Ok(findEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Добавление нового события
        /// </summary>
        /// <param name="eventModel">Принимает модель события</param>
        /// <returns>Возвращает 201 с ссылкой на созданное событие</returns>
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] EventModel eventModel)
        {
            try
            {
                int newIdEvent = await _eventService.Create(eventModel);
                return CreatedAtAction(nameof(GetEventById), new { id = newIdEvent }, eventModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Метод изменения существующего события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <param name="eventModel">Принимает новую модель события из body</param>
        /// <returns>Возвращает статус 200 OK c изменённым элементом, либо 400 с описанием ошибки</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditEvent([FromRoute] int id, [FromBody] EventModel eventModel)
        {
            try
            {
                if (id != eventModel.id)
                    return BadRequest("Проблема в сущности и в запросе! Проверьте правильность данных!");

                var updatedEvent = await _eventService.Update(eventModel);
                return Ok(updatedEvent);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("не найден", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Метод удаления события
        /// </summary>
        /// <param name="id">Принимает существующий id из списка событий из query</param>
        /// <returns>возвращает статус 204 в случае успеха, либо 400 с описанием ошибки</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] int id)
        {
            try
            {
                var findEvent = await _eventService.FindById(id);

                if (findEvent == null)
                    return NotFound($"Event c id: {id} не найден!");

                await _eventService.Delete(findEvent);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
