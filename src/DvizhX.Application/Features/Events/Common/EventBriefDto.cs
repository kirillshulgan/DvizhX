using DvizhX.Domain.Enums;

namespace DvizhX.Application.Features.Events.Common
{
    public record EventBriefDto(
        Guid Id,
        string Title,
        DateTime StartDate,
        EventStatus Status,
        ParticipantRole MyRole
    );
}
