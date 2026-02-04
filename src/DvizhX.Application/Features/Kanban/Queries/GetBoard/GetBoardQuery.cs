using DvizhX.Application.Features.Kanban.Common;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Queries.GetBoard
{
    public record GetBoardQuery(Guid EventId) : IRequest<BoardDto>;
}
