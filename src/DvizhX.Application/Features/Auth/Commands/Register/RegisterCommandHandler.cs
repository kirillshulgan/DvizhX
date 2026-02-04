using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Auth.Common;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<RegisterCommand, AuthenticationResult>
    {
        public async Task<AuthenticationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Проверка уникальности (теперь через метод репозитория)
            if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
            {
                throw new Exception("User with this email already exists.");
            }

            // 2. Создание юзера
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHasher.Hash(request.Password),
                AvatarUrl = null
            };

            // Сохранение через репозиторий
            await userRepository.AddAsync(user, cancellationToken);

            // 3. Генерация токена
            var (accessToken, jti) = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Email);
            var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

            // Сохраняем Refresh Token в БД
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
