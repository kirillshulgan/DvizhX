using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Events.Common;
using MediatR;

namespace DvizhX.Application.Features.Events.Queries.GetList
{
    public class GetEventsListQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetEventsListQuery, List<EventBriefDto>>
    {
        public async Task<List<EventBriefDto>> Handle(GetEventsListQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId
                         ?? throw new UnauthorizedAccessException();

            var events = await eventRepository.GetByUserIdAsync(userId, cancellationToken);

            return events.Select(e => new EventBriefDto(
                e.Id,
                e.Title,
                e.StartDate,
                e.Status,
                e.Participants.First(p => p.UserId == userId).Role // Моя роль
            )).ToList();
        }
    }
}
