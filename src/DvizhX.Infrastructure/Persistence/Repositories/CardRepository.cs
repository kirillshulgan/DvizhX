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
    }
}
