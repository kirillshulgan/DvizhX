using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using DvizhX.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DvizhX.Application.Features.Auth.Commands.TelegramAuth
{
    public class TelegramAuthCommandHandler(
        IConfiguration configuration,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<TelegramAuthCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(TelegramAuthCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            // 1. Проверка BotToken
            var botToken = configuration["TelegramSettings:BotToken"];
            if (string.IsNullOrEmpty(botToken))
                throw new Exception("Telegram BotToken is not configured on server.");

            // 2. Проверка актуальности (5 минут)
            if (!long.TryParse(data.AuthDate, out var authDateUnix))
                throw new Exception("Invalid auth_date format");

            var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix);
            if (DateTimeOffset.UtcNow - authDate > TimeSpan.FromMinutes(5))
                throw new Exception("Telegram auth data expired. Try again.");

            // 3. Сборка строки для проверки (data-check-string)
            // Важно: Порядок ключей должен быть алфавитным!
            // Важно: Исключаем пустые значения и сам hash
            var dataParams = new Dictionary<string, string?>
            {
                { "auth_date", data.AuthDate },
                { "first_name", data.FirstName },
                { "id", data.Id.ToString() },
                { "last_name", data.LastName },
                { "photo_url", data.PhotoUrl },
                { "username", data.Username }
            };

            // Фильтруем null и строим строку "key=value\n"
            var checkString = string.Join('\n', dataParams
                .Where(x => !string.IsNullOrEmpty(x.Value)) // Исключаем null
                .OrderBy(x => x.Key)                        // Сортируем
                .Select(x => $"{x.Key}={x.Value}"));

            // 4. Вычисление HMAC-SHA256
            // Секретный ключ = SHA256 хеш от токена бота (байт, не строки!)
            using var sha256 = SHA256.Create();
            var secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(botToken));

            // Подписываем checkString полученным ключом
            using var hmac = new HMACSHA256(secretKey);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(checkString));
            var calculatedHash = Convert.ToHexString(hashBytes).ToLower();

            // 5. Сравнение (case-insensitive для надежности)
            if (!string.Equals(calculatedHash, data.Hash, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Invalid Telegram signature. Data might be tampered.");
            }

            // --- БЛОК АВТОРИЗАЦИИ (Ваш код) ---

            // 6. Ищем или создаем юзера
            var user = await userRepository.GetByTelegramIdAsync(data.Id, cancellationToken);

            if (user == null)
            {
                // Генерируем уникальный Email-заглушку, если в БД стоит constraint
                var fakeEmail = $"{data.Id}@telegram.no-reply";

                // Имя пользователя: username > first_name > id
                var displayName = !string.IsNullOrWhiteSpace(data.Username)
                    ? data.Username
                    : data.FirstName ?? $"User{data.Id}";

                user = new User
                {
                    Username = displayName,
                    Email = fakeEmail,
                    AvatarUrl = data.PhotoUrl,

                    PasswordHash = null, // Пароля нет

                    TelegramId = data.Id
                };

                // Убедись, что в Entity User есть поле TelegramId (long?)
                await userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Обновляем аватарку, если сменилась
                if (!string.IsNullOrEmpty(data.PhotoUrl) && user.AvatarUrl != data.PhotoUrl)
                {
                    user.AvatarUrl = data.PhotoUrl;
                    await userRepository.UpdateAsync(user, cancellationToken);
                }
            }

            // 7. Токены
            var (accessToken, jti) = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Email);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            await refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            }, cancellationToken);

            return new AuthenticationResult(accessToken, refreshToken);
        }
    }
}
