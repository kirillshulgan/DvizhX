using MediatR;

namespace DvizhX.Application.Features.Events.Commands.JoinEvent
{
    public record JoinEventCommand(string InviteCode) : IRequest<Guid>;
}
