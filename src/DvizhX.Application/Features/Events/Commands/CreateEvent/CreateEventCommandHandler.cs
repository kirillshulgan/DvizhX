using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using DvizhX.Domain.Enums;
using MediatR;

namespace DvizhX.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateEventCommand, Guid>
    {
        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // Простейшая генерация инвайт-кода
            var inviteCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "").Replace("+", "").Substring(0, 8);

            var eventEntity = new Event
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                Status = EventStatus.Draft,
                InviteCode = inviteCode,
                CreatorId = userId,
                Participants = new List<EventParticipant>
            {
                // Создатель сразу становится владельцем
                new()
                {
                    UserId = userId,
                    Role = ParticipantRole.Owner,
                    IsAccepted = true
                }
            }
            };

            await eventRepository.AddAsync(eventEntity, cancellationToken);

            return eventEntity.Id;
        }
    }
}
