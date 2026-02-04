using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Register
{
    /// <summary> Данные для регистрации </summary>
    public record RegisterCommand(

        /// <summary> Уникальное имя пользователя </summary>
        /// <example>DvizhX_test_user</example>
        string Username,

        /// <summary> Email адрес (должен быть уникальным) </summary>
        /// <example>testuser1998@gmail.com</example>
        string Email,

        /// <summary> Пароль (минимум 6 символов) </summary>
        /// <example>TestUserPass</example>
        string Password
    ) : IRequest<AuthenticationResult>;
}
