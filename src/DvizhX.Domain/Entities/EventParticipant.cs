using DvizhX.Domain.Common;
using DvizhX.Domain.Enums;

namespace DvizhX.Domain.Entities
{
    public class EventParticipant : BaseEntity
    {
        public Guid EventId { get; set; }
        public Event? Event { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public ParticipantRole Role { get; set; } = ParticipantRole.Viewer;
        public bool IsAccepted { get; set; } = false; // Подтвердил участие?
    }
}
