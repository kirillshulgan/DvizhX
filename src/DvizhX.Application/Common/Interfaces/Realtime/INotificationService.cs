namespace DvizhX.Application.Common.Interfaces.Realtime
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string token, string title, string body);
        Task SendMulticastAsync(IEnumerable<string> tokens, string title, string body);
    }
}
