using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using DvizhX.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace DvizhX.Application.Features.Auth.Commands.TelegramAuth
{
    public class TelegramAuthCommandHandler(
    IConfiguration configuration,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<TelegramAuthCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(TelegramAuthCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            // 1. Проверка актуальности (чтобы не подсунули старый запрос)
            // Auth_date - это Unix timestamp
            if (long.TryParse(data.Auth_date, out var authDateUnix))
            {
                var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix);
                if (DateTimeOffset.UtcNow - authDate > TimeSpan.FromMinutes(5)) // Срок жизни 5 минут
                {
                    throw new Exception("Telegram auth data expired");
                }
            }

            // 2. Валидация подписи (HMAC)
            var botToken = configuration["TelegramSettings:BotToken"];
            if (string.IsNullOrEmpty(botToken)) throw new Exception("Telegram BotToken not configured");

            // Собираем строку данных: key=value, отсортированные по ключу
            var dataCheckDict = new SortedDictionary<string, string?>
        {
            { "auth_date", data.Auth_date },
            { "first_name", data.First_name },
            { "id", data.Id.ToString() },
            { "last_name", data.Last_name },
            { "photo_url", data.Photo_url },
            { "username", data.Username }
        };

            // Удаляем пустые значения (Telegram их не шлет в подписи)
            var dataCheckString = string.Join("\n", dataCheckDict
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={x.Value}"));

            // Считаем секретный ключ = SHA256(botToken)
            using var sha256 = SHA256.Create();
            var secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(botToken));

            // Считаем HMAC от dataCheckString
            using var hmac = new HMACSHA256(secretKey);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
            var hashHex = Convert.ToHexString(hashBytes).ToLower();

            if (hashHex != data.Hash)
            {
                throw new Exception("Invalid Telegram signature");
            }

            // 3. Логин / Регистрация
            var user = await userRepository.GetByTelegramIdAsync(data.Id, cancellationToken);

            if (user == null)
            {
                // Т.к. Telegram не отдает Email, генерируем фейковый или оставляем пустым (если разрешить в БД)
                // В нашем случае Email required, поэтому генерируем заглушку
                var fakeEmail = $"{data.Id}@telegram.dvizhx.local";

                user = new User
                {
                    Username = data.Username ?? data.First_name ?? $"User{data.Id}",
                    Email = fakeEmail,
                    TelegramId = data.Id,
                    AvatarUrl = data.Photo_url,
                    PasswordHash = null
                };
                await userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Обновляем инфу
                if (data.Photo_url != null && user.AvatarUrl != data.Photo_url)
                {
                    user.AvatarUrl = data.Photo_url;
                    await userRepository.UpdateAsync(user, cancellationToken);
                }
            }

            // 4. Генерация токенов
            var (accessToken, jti) = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Email);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsUsed = false,
                IsRevoked = false
            };

            await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            return new AuthenticationResult(accessToken, refreshToken);
        }
    }
}
