using DvizhX.Application.Common.Interfaces.Realtime;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace DvizhX.Infrastructure.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly ILogger<FirebaseNotificationService> _logger;

        public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger)
        {
            _logger = logger;

            // Инициализация Firebase (Singleton)
            if (FirebaseApp.DefaultInstance == null)
            {
                // Путь к JSON-ключу (лучше вынести в конфиг)
                var keyPath = Path.Combine(AppContext.BaseDirectory, "dvizhx-b5baf-firebase-adminsdk-fbsvc-b4302ed9fd.json");

                if (File.Exists(keyPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(keyPath)
                    });
                }
                else
                {
                    _logger.LogWarning($"Firebase key not found at {keyPath}. Push notifications disabled.");
                }
            }
        }

        public async Task SendNotificationAsync(string token, string title, string body)
        {
            if (FirebaseApp.DefaultInstance == null) return;

            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                // Опционально: данные (data payload) для обработки в фоне
                Data = new Dictionary<string, string>()
                {
                    { "click_action", "FLUTTER_NOTIFICATION_CLICK" }, // Для мобилок
                    { "url", "/" } // Для PWA редиректа
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent message: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                // TODO: Если ошибка "InvalidRegistration", удалить токен из БД
            }
        }

        public async Task SendMulticastAsync(IEnumerable<string> tokens, string title, string body)
        {
            if (FirebaseApp.DefaultInstance == null || !tokens.Any()) return;

            var message = new MulticastMessage()
            {
                Tokens = tokens.ToList(),
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                _logger.LogCritical($"[PUSH DEBUG] Sent: {response.SuccessCount}, Failed: {response.FailureCount}");
                _logger.LogCritical($"{response.SuccessCount} messages were sent successfully");

                if (response.FailureCount > 0)
                {
                    foreach (var resp in response.Responses.Where(r => !r.IsSuccess))
                    {
                        _logger.LogError($"[PUSH ERROR] {resp.Exception.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending multicast notification");
            }
        }
    }
}
