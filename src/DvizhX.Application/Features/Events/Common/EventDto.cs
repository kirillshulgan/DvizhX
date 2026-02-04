using DvizhX.Domain.Enums;

namespace DvizhX.Application.Features.Events.Common
{
    public record EventDto(
        Guid Id,
        string Title,
        string? Description,
        DateTime StartDate,
        EventStatus Status,
        string InviteCode,
        string InviteLink,
        ParticipantRole MyRole,
        int ParticipantsCount
    );
}
