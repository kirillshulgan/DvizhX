using DvizhX.Application.Features.Auth.Common;
using MediatR;

namespace DvizhX.Application.Features.Auth.Commands.TelegramAuth
{
    public record TelegramAuthCommand(TelegramAuthDto Data) : IRequest<AuthenticationResult>;
}
