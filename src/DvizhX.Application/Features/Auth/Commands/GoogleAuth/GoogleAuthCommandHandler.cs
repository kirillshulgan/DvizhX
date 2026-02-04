using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.GoogleAuth
{
    public class GoogleAuthCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IGoogleTokenValidator googleTokenValidator)
    : IRequestHandler<GoogleAuthCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(GoogleAuthCommand request, CancellationToken cancellationToken)
        {
            // 1. Валидация через интерфейс
            var payload = await googleTokenValidator.ValidateAsync(request.IdToken);

            // 2. Ищем пользователя (код тот же, но payload теперь твой рекорд)
            var user = await userRepository.GetByGoogleIdAsync(payload.Subject, cancellationToken);

            if (user == null)
            {
                // Если не нашли по GoogleId, ищем по Email (возможно, он уже регистрировался обычно)
                user = await userRepository.GetByEmailAsync(payload.Email, cancellationToken);

                if (user != null)
                {
                    // Привязываем Google аккаунт к существующему
                    user.GoogleId = payload.Subject;
                    // Обновляем аватар, если нет
                    if (string.IsNullOrEmpty(user.AvatarUrl)) user.AvatarUrl = payload.Picture;
                    await userRepository.UpdateAsync(user, cancellationToken);
                }
                else
                {
                    // Создаем нового
                    user = new User
                    {
                        Username = payload.Name ?? payload.Email.Split('@')[0],
                        Email = payload.Email,
                        GoogleId = payload.Subject,
                        AvatarUrl = payload.Picture,
                        PasswordHash = null // Пароля нет
                    };
                    await userRepository.AddAsync(user, cancellationToken);
                }
            }

            // 3. Генерируем токены (Дублирование логики, но пока OK)
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
