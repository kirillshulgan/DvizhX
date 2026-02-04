using MediatR;

namespace DvizhX.Application.Features.Events.Commands.JoinEvent
{
    /// <summary> Данные для вступления в событие </summary>
    public record JoinEventCommand(
        /// <summary>Инвайт код события</summary>
        /// <example>x76S09asd</example>
        string InviteCode
    ) : IRequest<Guid>;
}
