using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class UserRepository(ApplicationDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}
