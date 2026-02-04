using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DvizhX.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler(
    IJwtTokenValidator jwtTokenValidator,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Парсим истекший Access Token
            var principal = jwtTokenValidator.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal is null)
            {
                throw new UnauthorizedAccessException("Invalid access token.");
            }

            // 2. Извлекаем Claims
            //var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub); //TODO: Почему не работает?
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            //var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value; // ID токена

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
            {
                throw new UnauthorizedAccessException("Invalid token claims.");
            }

            // 3. Ищем Refresh Token в БД
            var storedRefreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (storedRefreshToken is null)
            {
                throw new UnauthorizedAccessException("Refresh token not found.");
            }

            // 4. Валидация Refresh Token
            if (storedRefreshToken.IsUsed)
            {
                // ВАЖНО: Если токен уже использован, это может быть признаком атаки (Token Replay)
                // Лучше отозвать ВСЕ токены этого юзера
                throw new UnauthorizedAccessException("Refresh token has already been used.");
            }

            if (storedRefreshToken.IsRevoked)
            {
                throw new UnauthorizedAccessException("Refresh token has been revoked.");
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            if (storedRefreshToken.JwtId != jti)
            {
                throw new UnauthorizedAccessException("Refresh token does not match access token.");
            }

            if (storedRefreshToken.UserId.ToString() != userId)
            {
                throw new UnauthorizedAccessException("Token user mismatch.");
            }

            // 5. Помечаем старый Refresh Token как использованный (Token Rotation)
            storedRefreshToken.IsUsed = true;
            await refreshTokenRepository.UpdateAsync(storedRefreshToken, cancellationToken);

            // 6. Генерируем новую пару токенов
            var user = storedRefreshToken.User!;
            var (newAccessToken, newJti) = jwtTokenGenerator.GenerateAccessToken(
                user.Id,
                user.Username,
                user.Email
            );
            var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();

            // 7. Сохраняем новый Refresh Token
            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = newRefreshToken,
                JwtId = newJti,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6), // Живет 6 месяцев
                IsUsed = false,
                IsRevoked = false
            };

            await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            return new AuthenticationResult(newAccessToken, newRefreshToken);
        }
    }
}
