using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }

        // Auth fields (упрощенно, в реальности IdentityUser)
        public required string PasswordHash { get; set; }
        public ICollection<EventParticipant> EventParticipations { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
