using System.ComponentModel.DataAnnotations;

namespace DvizhX.Application.Features.Notifications.Common
{
    public class SubscribeDto
    {
        [Required]
        public string Token { get; set; } = string.Empty; // FCM токен

        public string? DeviceType { get; set; } // "Web", "Android", "iOS"
    }
}
