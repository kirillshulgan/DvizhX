using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace DvizhX.Application.Features.Events.Commands.RegenerateInvite
{
    public class RegenerateInviteCodeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IConfiguration configuration)
    : IRequestHandler<RegenerateInviteCodeCommand, string>
    {
        public async Task<string> Handle(RegenerateInviteCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            var eventEntity = await eventRepository.GetByIdWithParticipantsAsync(request.EventId, cancellationToken);
            if (eventEntity == null) throw new Exception("Event not found");

            // Проверка прав: только Owner или Admin
            var participant = eventEntity.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || (participant.Role != ParticipantRole.Owner && participant.Role != ParticipantRole.Admin))
            {
                throw new Exception("Access denied"); // Forbidden
            }

            // Генерируем новый код
            var newCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "").Replace("+", "").Replace("=", "").Substring(0, 8);

            eventEntity.InviteCode = newCode;
            await eventRepository.UpdateAsync(eventEntity, cancellationToken);

            // Формируем полную ссылку
            // Базовый URL лучше брать из конфига, но для дева можно хардкод или Request.Host
            var baseUrl = configuration["App:FrontendUrl"] ?? throw new InvalidOperationException("App:FrontendUrl is not configured.");
            return $"{baseUrl}/join/{newCode}";
        }
    }
}
