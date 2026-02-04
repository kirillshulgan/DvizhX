using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class BoardRepository(ApplicationDbContext dbContext) : BaseRepository<Board>(dbContext), IBoardRepository
    {
        public async Task<Board?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Boards
                .Include(b => b.Columns.OrderBy(c => c.OrderIndex))
                    .ThenInclude(c => c.Cards.OrderBy(card => card.OrderIndex))
                        .ThenInclude(card => card.AssignedUser) // Чтобы знать имя исполнителя
                .FirstOrDefaultAsync(b => b.EventId == eventId, cancellationToken);
        }
    }
}
