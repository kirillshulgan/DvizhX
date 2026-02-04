using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LoginCommand, AuthenticationResult> // Исправили
    {
        public async Task<AuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Ищем юзера
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null)
            {
                throw new Exception("Invalid credentials."); // Не палим, что email не найден
            }

            // 2. Проверяем пароль
            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid credentials.");
            }

            // 3. Генерируем токены
            var (accessToken, jti) = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Email);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            // 4. Сохраняем Refresh Token в БД
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
