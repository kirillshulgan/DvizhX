namespace DvizhX.Application.Features.Auth.Common
{
    public record UserDto(Guid Id, string Username, string Email, string? AvatarUrl);
}
