using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class EventRepository(ApplicationDbContext dbContext) : BaseRepository<Event>(dbContext), IEventRepository
    {
        public async Task<Event?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Events
                .Include(e => e.Participants) // Грузим участников, чтобы проверить доступ
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Events
                .Include(e => e.Participants)
                .Where(e => e.Participants.Any(p => p.UserId == userId)) // Только те, где я участвую
                .OrderBy(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }
    }
}
