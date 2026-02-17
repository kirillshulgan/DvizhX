using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DvizhX.Api.Hubs
{
    [Authorize]
    public class KanbanHub : Hub
    {
        // Клиент вызывает этот метод, когда открывает страницу доски
        public async Task JoinBoard(Guid eventId)
        {
            // В реальном проекте тут тоже надо проверить права доступа к ивенту!
            var groupName = GetGroupName(eventId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Клиент вызывает, когда уходит
        public async Task LeaveBoard(Guid eventId)
        {
            var groupName = GetGroupName(eventId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public static string GetGroupName(Guid eventId) => $"board_{eventId}";
    }
}
