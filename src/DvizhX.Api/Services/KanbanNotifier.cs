using DvizhX.Api.Hubs;
using DvizhX.Application.Common.Interfaces.Realtime;
using DvizhX.Application.Features.Kanban.Common;
using Microsoft.AspNetCore.SignalR;

namespace DvizhX.Api.Services
{
    public class KanbanNotifier(IHubContext<KanbanHub> hubContext) : IKanbanNotifier
    {
        public async Task CardCreatedAsync(Guid eventId, CardDto card)
        {
            var group = KanbanHub.GetGroupName(eventId);
            // Шлем событие "CardCreated" всем в группе
            await hubContext.Clients.Group(group).SendAsync("CardCreated", card);
        }

        public async Task CardMovedAsync(Guid eventId, Guid cardId, Guid newColumnId, int newIndex)
        {
            var group = KanbanHub.GetGroupName(eventId);
            await hubContext.Clients.Group(group).SendAsync("CardMoved", new { cardId, newColumnId, newIndex });
        }

        public async Task CardUpdatedAsync(Guid eventId, Guid cardId, string cardTitle, string cardDescription)
        {
            var group = KanbanHub.GetGroupName(eventId);
            await hubContext.Clients.Group(group).SendAsync("CardUpdated", new { cardId, cardTitle, cardDescription });
        }

        public async Task CardDeletedAsync(Guid eventId, Guid cardId)
        {
            var group = KanbanHub.GetGroupName(eventId);
            await hubContext.Clients.Group(group).SendAsync("CardDeleted", new { cardId });
        }
    }
}
