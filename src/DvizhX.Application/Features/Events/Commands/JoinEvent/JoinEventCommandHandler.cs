using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using DvizhX.Domain.Enums;
using MediatR;

namespace DvizhX.Application.Features.Events.Commands.JoinEvent
{
    public class JoinEventCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<JoinEventCommand, Guid>
    {
        public async Task<Guid> Handle(JoinEventCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Ищем событие
            var eventEntity = await eventRepository.GetByInviteCodeAsync(request.InviteCode, cancellationToken);

            if (eventEntity == null)
                throw new Exception("Event not found or invalid code");

            // 2. Проверяем, не участник ли уже
            if (eventEntity.Participants.Any(p => p.UserId == userId))
            {
                // Уже участник - просто возвращаем ID, чтобы фронт мог сделать редирект
                return eventEntity.Id;
            }

            // 3. Добавляем
            var participant = new EventParticipant
            {
                EventId = eventEntity.Id,
                UserId = userId,
                Role = ParticipantRole.Member, // По дефолту даем права участника
                IsAccepted = true
            };

            // Тут внимание: можно добавить через коллекцию eventEntity.Participants.Add(participant)
            // Но EF Core трекает изменения, если мы их загрузили через Include.
            //eventEntity.Participants.Add(participant);

            // Сохраняем (EF обновит связь)
            //await eventRepository.UpdateAsync(eventEntity, cancellationToken);

            await eventRepository.AddParticipantAsync(participant, cancellationToken);

            return eventEntity.Id;
        }
    }
}
