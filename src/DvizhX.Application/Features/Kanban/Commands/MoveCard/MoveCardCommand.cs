using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.MoveCard
{
    public record MoveCardCommand(
        Guid CardId,
        Guid TargetColumnId, // Куда бросили
        int NewOrderIndex    // На какую позицию (0 - верх)
    ) : IRequest;
}
