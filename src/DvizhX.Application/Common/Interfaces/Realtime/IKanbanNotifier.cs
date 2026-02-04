using DvizhX.Application.Features.Kanban.Common;

namespace DvizhX.Application.Common.Interfaces.Realtime
{
    public interface IKanbanNotifier
    {
        Task CardCreatedAsync(Guid eventId, CardDto card);
        Task CardMovedAsync(Guid eventId, Guid cardId, Guid newColumnId, int newIndex);
    }
}
