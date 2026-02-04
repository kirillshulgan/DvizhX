using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface ICardRepository : IRepository<Card>
    {
        // Найти колонку со всей иерархией (Board -> Event -> Participants)
        Task<BoardColumn?> GetColumnWithHierarchyAsync(Guid columnId, CancellationToken cancellationToken = default);

        // Получить максимальный OrderIndex в колонке
        Task<int> GetMaxOrderIndexAsync(Guid columnId, CancellationToken cancellationToken = default);

        // Получить карту с данными о текущей колонке
        Task<Card?> GetByIdWithColumnAsync(Guid id, CancellationToken cancellationToken = default);

        // Сдвинуть индексы вниз (освободить место): index >= X -> index + 1
        Task ShiftIndexesDownAsync(Guid columnId, int startIndex, CancellationToken cancellationToken = default);

        // Сдвинуть индексы вверх (закрыть дырку): index > X -> index - 1
        Task ShiftIndexesUpAsync(Guid columnId, int startIndex, CancellationToken cancellationToken = default);

        // Получить карточки колонки, отсортированные по индексу (для пересчета)
        Task<List<Card>> GetCardsByColumnIdAsync(Guid columnId, CancellationToken cancellationToken = default);
    }
}
