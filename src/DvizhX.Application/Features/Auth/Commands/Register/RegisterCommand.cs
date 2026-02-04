using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(string Username, string Email, string Password) : IRequest<AuthenticationResult>;
}
