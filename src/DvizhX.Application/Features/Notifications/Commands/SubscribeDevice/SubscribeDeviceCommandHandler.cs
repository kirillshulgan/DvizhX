using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Notifications.Commands.SubscribeDevice
{
    public class SubscribeDeviceCommandHandler : IRequestHandler<SubscribeDeviceCommand>
    {
        private readonly IDeviceTokenRepository _repository;

        public SubscribeDeviceCommandHandler(IDeviceTokenRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(SubscribeDeviceCommand request, CancellationToken cancellationToken)
        {
            // Создаем или обновляем сущность токена
            var tokenEntity = new DeviceToken
            {
                UserId = request.UserId,
                Token = request.Token,
                DeviceType = request.DeviceType ?? "Web", // По умолчанию Web
                LastUsed = DateTime.UtcNow
            };

            // Вызываем метод репозитория, который мы написали ранее
            // (Он проверит существование и обновит LastUsed, если токен уже есть)
            await _repository.AddOrUpdateAsync(tokenEntity, cancellationToken);

            // Возвращаем Unit (пустой результат), так как IRequest без типа
            return;
        }
    }
}
