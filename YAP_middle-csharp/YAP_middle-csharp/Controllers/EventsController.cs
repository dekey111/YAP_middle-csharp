using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                return Ok(await _eventService.GetAll());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            try
            {
                var findEvent = await _eventService.GetById(id);
                if (findEvent == null)
                    return NotFound($"Event c id: {id} не найден!");

                return Ok(findEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent(EventModel eventModel)
        {
            try
            {
                await Validation(eventModel);
                var newEvent = await _eventService.Add(eventModel);
                return CreatedAtAction(nameof(GetEventById), new { id = newEvent.id }, newEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditEvent(int id, EventModel eventModel)
        {
            try
            {
                await Validation(eventModel);
                var editingEvent = await _eventService.Edit(eventModel);
                return Ok(editingEvent);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("не найден", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                await _eventService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("не найден", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        private async Task Validation(EventModel eventModel)
        {
            if(eventModel == null)
                throw new ArgumentNullException(nameof(eventModel));

            if (string.IsNullOrEmpty(eventModel.Title))
                throw new Exception("Поле заголовок не может быть пустым!");

            if (eventModel.StartAt == default)
                throw new Exception("Поле дата начала не может быть пустым!");

            if (eventModel.EndAt == default)
                throw new Exception("Поле дата окончания не может быть пустым!");

            if (eventModel.StartAt > eventModel.EndAt)
                throw new Exception("Дата окончания не может быть раньше дате начала!");
        }
    }
}
