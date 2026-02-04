using DvizhX.Domain.Enums;

namespace DvizhX.Application.Features.Events.Common
{
    public record EventBriefDto(
        Guid Id,
        string Title,
        string Description,
        DateTime StartDate,
        EventStatus Status,
        ParticipantRole MyRole
    );
}
