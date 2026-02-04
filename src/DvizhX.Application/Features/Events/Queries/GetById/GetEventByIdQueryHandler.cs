using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Events.Common;
using MediatR;

namespace DvizhX.Application.Features.Events.Queries.GetById
{
    public class GetEventByIdQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetEventByIdQuery, EventDto>
    {
        public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId
                         ?? throw new UnauthorizedAccessException();

            var entity = await eventRepository.GetByIdWithParticipantsAsync(request.Id, cancellationToken);

            if (entity == null)
                throw new Exception("Event not found"); // Лучше кастомный NotFoundException

            // Проверка доступа: Я должен быть в списке участников
            var myParticipantRecord = entity.Participants.FirstOrDefault(p => p.UserId == userId);

            if (myParticipantRecord == null)
                throw new Exception("Access denied"); // Лучше кастомный ForbiddenException

            return new EventDto(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.StartDate,
                entity.Status,
                entity.InviteCode,
                myParticipantRecord.Role,
                entity.Participants.Count
            );
        }
    }
}
