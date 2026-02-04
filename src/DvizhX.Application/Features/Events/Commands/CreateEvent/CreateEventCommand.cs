using MediatR;

namespace DvizhX.Application.Features.Events.Commands.CreateEvent
{
    /// <summary> Данные для создания нового события </summary>
    public record CreateEventCommand(

        /// <summary>Название события (например, "Поездка на шашлыки")</summary>
        /// <example>Выходные на природе</example>
        string Title,

        /// <summary>Описание события (необязательно)</summary>
        /// <example>Едем на дачу, берем мясо, овощи, гитару</example>
        string? Description,

        /// <summary>Дата и время начала события</summary>
        /// <example>2026-02-10T10:00:00Z</example>
        DateTime StartDate
    ) : IRequest<Guid>;
}
