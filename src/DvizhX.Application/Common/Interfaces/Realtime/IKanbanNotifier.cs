using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Realtime
{
    public interface IKanbanNotifier
    {
        Task CardCreatedAsync(Guid eventId, CardDto card);
        Task CardMovedAsync(Guid eventId, Guid cardId, Guid newColumnId, int newIndex);
        Task CardUpdatedAsync(Guid eventId, Guid cardId, string cardTitle, string cardDescription);
        Task CardDeletedAsync(Guid eventId, Guid cardId);
    }
}
