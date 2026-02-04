using DvizhX.Application.Features.Events.Commands.CreateEvent;
using DvizhX.Application.Features.Events.Commands.JoinEvent;
using DvizhX.Application.Features.Events.Commands.RegenerateInvite;
using DvizhX.Application.Features.Events.Common;
using DvizhX.Application.Features.Events.Queries.GetById;
using DvizhX.Application.Features.Events.Queries.GetList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DvizhX.Api.Controllers
{
    /// <summary> Управление событиями (тусовки, встречи, поездки) </summary>
    /// <remarks> Методы для управления событиями </remarks>
    [Tags("2. События")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Получить список моих событий
        /// </summary>
        /// <remarks>
        /// Возвращает события, в которых текущий пользователь является участником.
        /// Отсортированы по дате начала.
        /// </remarks>
        /// <response code="200">Список событий успешно получен</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<EventBriefDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetList()
        {
            var result = await mediator.Send(new GetEventsListQuery());
            return Ok(result);
        }

        /// <summary>
        /// Получить детали события
        /// </summary>
        /// <param name="id">ID события</param>
        /// <remarks>
        /// Возвращает подробную информацию о событии. 
        /// Доступ имеют только участники.
        /// </remarks>
        /// <response code="200">Событие найдено</response>
        /// <response code="403">Доступ запрещён (вы не участник)</response>
        /// <response code="404">Событие не найдено</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await mediator.Send(new GetEventByIdQuery(id));
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message == "Access denied")
            {
                return Forbid();
            }
            catch (Exception ex) when (ex.Message == "Event not found")
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Создать новое событие
        /// </summary>
        /// <param name="command">Данные события</param>
        /// <remarks>
        /// Создаёт новое событие. Создатель автоматически становится владельцем (Owner).
        /// Возвращает ID созданного события.
        /// </remarks>
        /// <response code="201">Событие создано</response>
        /// <response code="400">Невалидные данные</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateEventCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Вступить в событие по коду приглашения
        /// </summary>
        /// <param name="code">Уникальный код приглашения (8 символов)</param>
        /// <remarks>
        /// Позволяет присоединиться к событию по ссылке-приглашению.
        /// Пользователь получает роль "Участник" (Member).
        /// </remarks>
        /// <response code="200">Успешно вступили (или уже были участником)</response>
        /// <response code="404">Неверный код или событие не найдено</response>
        [HttpPost("join/{code}")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Join(string code)
        {
            var eventId = await mediator.Send(new JoinEventCommand(code));
            return Ok(new { EventId = eventId });
        }

        /// <summary>
        /// Получить/Сгенерировать новую ссылку-приглашение
        /// </summary>
        /// <remarks>
        /// Сбрасывает старый код приглашения и возвращает новую ссылку.
        /// Доступно только для Owner и Admin.
        /// </remarks>
        [HttpPost("{id}/regenerate-invite")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegenerateInvite(Guid id)
        {
            try
            {
                var link = await mediator.Send(new RegenerateInviteCodeCommand(id));
                return Ok(new { InviteLink = link });
            }
            catch (Exception ex) when (ex.Message == "Access denied")
            {
                return Forbid();
            }
        }
    }
}
