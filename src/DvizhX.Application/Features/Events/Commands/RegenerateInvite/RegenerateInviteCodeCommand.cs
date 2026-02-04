using MediatR;

namespace DvizhX.Application.Features.Events.Commands.RegenerateInvite
{
    // Возвращаем новую ссылку (полную строку)
    /// <summary> Данные для создания новой инвайт ссылки </summary>
    public record RegenerateInviteCodeCommand(Guid EventId) : IRequest<string>;
}
