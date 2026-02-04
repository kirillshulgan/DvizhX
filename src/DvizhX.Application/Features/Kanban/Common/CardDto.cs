namespace DvizhX.Application.Features.Kanban.Common
{
    public record CardDto(
        Guid Id,
        string Title,
        string? Description,
        int OrderIndex,
        Guid? AssignedUserId,
        string? AssignedUserAvatar
    );
}
