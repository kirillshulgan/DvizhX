using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<Event?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Event?> GetByInviteCodeAsync(string code, CancellationToken cancellationToken = default);
        Task AddParticipantAsync(EventParticipant participant, CancellationToken cancellationToken = default);
    }
}
