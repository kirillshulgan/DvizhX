using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, string>
    {
        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Ищем юзера
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

            // В продакшене лучше возвращать одинаковую ошибку "Invalid credentials", чтобы не палить существование почты
            if (user is null)
            {
                throw new Exception("User not found.");
            }

            // 2. Проверяем пароль
            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid password.");
            }

            // 3. Генерируем токен
            var token = jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Email);

            return token;
        }
    }
}
