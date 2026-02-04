using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class CardRepository(ApplicationDbContext dbContext) : BaseRepository<Card>(dbContext), ICardRepository
    {
        public async Task<BoardColumn?> GetColumnWithHierarchyAsync(Guid columnId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Columns
                .Include(c => c.Board)
                    .ThenInclude(b => b.Event)
                        .ThenInclude(e => e.Participants)
                .FirstOrDefaultAsync(c => c.Id == columnId, cancellationToken);
        }

        public async Task<int> GetMaxOrderIndexAsync(Guid columnId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Cards
                .Where(c => c.ColumnId == columnId)
                .MaxAsync(c => (int?)c.OrderIndex, cancellationToken) ?? -1;
        }

        public async Task<Card?> GetByIdWithColumnAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Cards
                .Include(c => c.Column)
                    .ThenInclude(col => col.Board)
                        .ThenInclude(b => b.Event)
                            .ThenInclude(e => e.Participants) // Нужно для проверки прав
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<Card>> GetCardsByColumnIdAsync(Guid columnId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Cards
                .Where(c => c.ColumnId == columnId)
                .OrderBy(c => c.OrderIndex)
                .ToListAsync(cancellationToken);
        }

        // Оптимизация: выполняем SQL update без загрузки сущностей
        // "Сдвинь всех вниз, чтобы освободить место startIndex"
        public async Task ShiftIndexesDownAsync(Guid columnId, int startIndex, CancellationToken cancellationToken = default)
        {
            await _dbContext.Cards
                .Where(c => c.ColumnId == columnId && c.OrderIndex >= startIndex)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.OrderIndex, c => c.OrderIndex + 1), cancellationToken);
        }

        // "Сдвинь всех вверх, чтобы закрыть дырку после startIndex"
        public async Task ShiftIndexesUpAsync(Guid columnId, int startIndex, CancellationToken cancellationToken = default)
        {
            await _dbContext.Cards
                .Where(c => c.ColumnId == columnId && c.OrderIndex > startIndex)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.OrderIndex, c => c.OrderIndex - 1), cancellationToken);
        }
    }
}
