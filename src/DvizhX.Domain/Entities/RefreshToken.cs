using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public required string Token { get; set; }
        public required string JwtId { get; set; } // ID access токена, с которым связан этот refresh
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } // Если юзер вышел или токен украли
        public bool IsUsed { get; set; } // Одноразовый токен (Rotation)

        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
