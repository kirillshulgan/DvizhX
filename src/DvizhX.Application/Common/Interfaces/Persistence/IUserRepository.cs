using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    }
}
