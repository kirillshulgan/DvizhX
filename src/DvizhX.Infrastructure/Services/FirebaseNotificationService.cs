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

            // 1. Читаем файл как текст
            string jsonContent;
            try
            {
                // Путь к файлу внутри контейнера
                var keyPath = Path.Combine(AppContext.BaseDirectory, "dvizhx-b5baf-firebase-adminsdk-fbsvc-b4302ed9fd.json");
                // Или используй точное имя файла, которое ты видишь в папке /app

                // Если ты не уверен в пути, попробуй найти любой json в папке
                if (!File.Exists(keyPath))
                {
                    var files = Directory.GetFiles(AppContext.BaseDirectory, "*firebase*.json");
                    if (files.Any()) keyPath = files.First();
                }

                jsonContent = File.ReadAllText(keyPath);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[CRITICAL] Failed to read Firebase JSON: {ex.Message}");
                throw;
            }

            // 2. 🔥 МАГИЧЕСКОЕ ИСПРАВЛЕНИЕ 🔥
            // Если в строке встречаются экранированные двойные слэши (\\n), превращаем их в обычные (\n)
            if (jsonContent.Contains("\\n"))
            {
                _logger.LogInformation("Fixing escaped newlines in Firebase Key...");
                jsonContent = jsonContent.Replace("\\n", "\n");
            }

            // 3. Создаем FirebaseApp из ИСПРАВЛЕННОЙ строки
            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions
            //    {
            //        Credential = GoogleCredential.FromJson(jsonContent)
            //    });
            //}
            if (FirebaseApp.DefaultInstance == null)
            {
                // Явно говорим, что мы ожидаем Service Account (это безопаснее)
                var serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(jsonContent);

                // Конвертируем в GoogleCredential
                var googleCredential = serviceAccountCredential.ToGoogleCredential();

                FirebaseApp.Create(new AppOptions
                {
                    Credential = googleCredential
                });
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
