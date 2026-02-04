using MediatR;

namespace DvizhX.Application.Features.Events.Commands.CreateEvent
{
    public record CreateEventCommand(
        string Title,
        string? Description,
        DateTime StartDate
    ) : IRequest<Guid>;
}
