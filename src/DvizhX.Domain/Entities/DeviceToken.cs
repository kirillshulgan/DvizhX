using DvizhX.Domain.Common;

namespace DvizhX.Domain.Entities
{
    public class DeviceToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Token { get; set; } // Сам FCM токен (длинная строка)
        public string DeviceType { get; set; } // "Android", "iOS", "Web"
        public DateTime LastUsed { get; set; }
    }
}
