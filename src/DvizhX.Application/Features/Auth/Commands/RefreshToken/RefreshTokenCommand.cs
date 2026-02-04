using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(

        /// <summary> Истекший Access Token (нужен для извлечения Claims) </summary>
        string AccessToken,

        /// <summary> Валидный Refresh Token </summary>
        string RefreshToken
    ) : IRequest<AuthenticationResult>;
}
