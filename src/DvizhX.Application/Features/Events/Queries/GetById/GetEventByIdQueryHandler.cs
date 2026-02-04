using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Events.Common;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace DvizhX.Application.Features.Events.Queries.GetById
{
    public class GetEventByIdQueryHandler(
        IConfiguration configuration,
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

            var baseUrl = configuration["App:FrontendUrl"] ?? throw new InvalidOperationException("App:FrontendUrl is not configured.");
            var inviteLink = $"{baseUrl}/join/{entity.InviteCode}";


            return new EventDto(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.StartDate,
                entity.Status,
                entity.InviteCode,
                inviteLink,
                myParticipantRecord.Role,
                entity.Participants.Count
            );
        }
    }
}
