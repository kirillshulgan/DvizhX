using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.GoogleAuth
{
    public record GoogleAuthCommand(string IdToken) : IRequest<AuthenticationResult>;
}
