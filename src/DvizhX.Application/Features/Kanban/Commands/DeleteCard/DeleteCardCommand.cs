using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.DeleteCard
{
    public record DeleteCardCommand(Guid CardId) : IRequest;
}
