using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<Board?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
