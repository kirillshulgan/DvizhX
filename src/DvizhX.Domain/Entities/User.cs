using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }


        public string? PasswordHash { get; set; }

        // External Providers
        public string? GoogleId { get; set; }
        public long? TelegramId { get; set; }

        public ICollection<EventParticipant> EventParticipations { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
