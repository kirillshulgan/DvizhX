using DvizhX.Application.Features.Kanban.Common;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.CreateCard
{
    // Возвращаем DTO созданной карточки, чтобы фронт сразу её отрисовал без перезагрузки
    /// <summary> Команда создания новой задачи </summary>
    public record CreateCardCommand(

        /// <summary> ID колонки, в которую добавить задачу </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        Guid ColumnId,

        /// <summary> Заголовок задачи </summary>
        /// <example>Купить 5кг маринованного мяса</example>
        string Title,

        /// <summary> Подробное описание (Markdown поддерживается на фронте) </summary>
        /// <example>Лучше брать шею, маринад классический. Не забыть лук!</example>
        string? Description
    ) : IRequest<CardDto>;
}
