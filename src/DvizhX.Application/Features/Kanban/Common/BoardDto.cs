namespace DvizhX.Application.Features.Kanban.Common
{
    public record BoardDto(Guid Id, Guid EventId, List<ColumnDto> Columns);
}
