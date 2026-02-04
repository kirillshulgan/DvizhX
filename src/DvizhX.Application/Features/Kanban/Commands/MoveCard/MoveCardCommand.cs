using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.MoveCard
{
    public record MoveCardCommand(
        Guid CardId,
        Guid TargetColumnId, // Куда перетащили (может совпадать с текущей)
        int NewOrderIndex    // На какую позицию (0 - в начало, 999 - в конец)
    ) : IRequest;
}
