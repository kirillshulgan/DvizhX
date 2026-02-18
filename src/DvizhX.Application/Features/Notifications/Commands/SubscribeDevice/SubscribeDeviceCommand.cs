using MediatR;

namespace DvizhX.Application.Features.Notifications.Commands.SubscribeDevice
{
    public record SubscribeDeviceCommand(
        Guid UserId,
        string Token,
        string? DeviceType
    ) : IRequest;
}
