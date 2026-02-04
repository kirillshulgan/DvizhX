using DvizhX.Application.Features.Events.Common;
using MediatR;

namespace DvizhX.Application.Features.Events.Queries.GetList
{
    public record GetEventsListQuery : IRequest<List<EventBriefDto>>;
}
