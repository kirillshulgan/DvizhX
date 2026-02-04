using DvizhX.Application.Features.Kanban.Commands.CreateCard;
using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Application.Features.Kanban.Queries.GetBoard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DvizhX.Api.Controllers
{
    /// <summary> Управление доской задач (Канбан) </summary>
    /// <remarks>
    /// Методы для работы с колонками и карточками.
    /// <para>
    /// ⚡ **Real-time:** Методы изменения данных (Create, Move) отправляют уведомления 
    /// всем участникам через SignalR Hub (`/hubs/kanban`).
    /// </para>
    /// </remarks>
    [Tags("3. Канбан доска")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Обязательно требуем токен
    public class KanbanController(IMediator mediator) : ControllerBase
    {
        /// <summary> Получить доску события </summary>
        /// <param name="eventId">ID события</param>
        /// <remarks>
        /// Возвращает полную структуру доски: колонки и карточки.
        /// Если доски нет, она создается автоматически с колонками "To Do", "In Progress", "Done".
        /// </remarks>
        /// <response code="200">Доска успешно получена</response>
        /// <response code="403">Вы не являетесь участником события</response>
        /// <response code="404">Событие не найдено</response>
        [HttpGet("{eventId}")]
        [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoard(Guid eventId)
        {
            try
            {
                var result = await mediator.Send(new GetBoardQuery(eventId));
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message == "Event not found")
            {
                return NotFound(new { Message = "Event not found" });
            }
            catch (Exception ex) when (ex.Message == "Access denied")
            {
                return Forbid(); // Возвращает 403 Forbidden
            }
        }

        /// <summary> Создать новую карточку </summary>
        /// <remarks>
        /// Добавляет карточку в указанную колонку (в самый низ).
        /// <br/>
        /// 🔔 Отправляет событие `CardCreated` всем подписчикам SignalR группы события.
        /// </remarks>
        /// <param name="command">Данные карточки</param>
        /// <response code="200">Карточка создана</response>
        /// <response code="400">Ошибка валидации</response>
        /// <response code="403">Нет прав на добавление (вы не участник)</response>
        [HttpPost("cards")]
        [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCard(CreateCardCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}
