using DvizhX.Application.Features.Events.Commands.CreateEvent;
using DvizhX.Application.Features.Events.Queries.GetById;
using DvizhX.Application.Features.Events.Queries.GetList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DvizhX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var result = await mediator.Send(new GetEventsListQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
    }
}
