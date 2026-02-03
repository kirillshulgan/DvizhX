using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<string>;
}
