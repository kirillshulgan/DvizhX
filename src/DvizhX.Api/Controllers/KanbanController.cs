using DvizhX.Application.Features.Kanban.Queries.GetBoard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DvizhX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Обязательно требуем токен
    public class KanbanController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{eventId}")]
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
    }
}
