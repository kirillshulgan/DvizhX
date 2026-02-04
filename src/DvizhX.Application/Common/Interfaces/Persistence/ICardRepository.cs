using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface ICardRepository : IRepository<Card>
    {
        // Найти колонку со всей иерархией (Board -> Event -> Participants)
        Task<BoardColumn?> GetColumnWithHierarchyAsync(Guid columnId, CancellationToken cancellationToken = default);

        // Получить максимальный OrderIndex в колонке
        Task<int> GetMaxOrderIndexAsync(Guid columnId, CancellationToken cancellationToken = default);
    }
}
