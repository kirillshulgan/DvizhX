using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.UpdateCard
{
    public record UpdateCardCommand(
        Guid CardId,
        string Title,
        string? Description
    ) : IRequest;
}
