using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthenticationResult>;
}
