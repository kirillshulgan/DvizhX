using DvizhX.Application.Features.Events.Common;
using MediatR;

namespace DvizhX.Application.Features.Events.Queries.GetById
{
    public record GetEventByIdQuery(Guid Id) : IRequest<EventDto>;
}
