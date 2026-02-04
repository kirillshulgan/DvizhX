namespace DvizhX.Application.Features.Kanban.Common
{
    public record ColumnDto(Guid Id, string Title, int OrderIndex, List<CardDto> Cards);
}
