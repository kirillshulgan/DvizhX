using DvizhX.Domain.Common;
using DvizhX.Domain.Enums;

namespace DvizhX.Domain.Entities
{
    public class Event : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Draft;

        // Уникальный код для инвайт-ссылки (например, "xK8sL2")
        public required string InviteCode { get; set; }

        public Guid CreatorId { get; set; }
        public User? Creator { get; set; }

        public ICollection<EventParticipant> Participants { get; set; } = [];
        public ICollection<Board> Boards { get; set; } = [];
    }
}
