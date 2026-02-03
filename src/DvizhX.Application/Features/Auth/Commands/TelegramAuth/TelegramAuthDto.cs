namespace DvizhX.Application.Features.Auth.Commands.TelegramAuth
{
    public record TelegramAuthDto(long Id, string FirstName, string Username, string PhotoUrl, string AuthDate, string Hash);
}
