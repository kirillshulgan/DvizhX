using DvizhX.Application.Features.Kanban.Common;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.CreateCard
{
    // Возвращаем DTO созданной карточки, чтобы фронт сразу её отрисовал без перезагрузки
    public record CreateCardCommand(
        Guid ColumnId,
        string Title,
        string? Description
    ) : IRequest<CardDto>;
}
