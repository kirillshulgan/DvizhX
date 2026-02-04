using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository(ApplicationDbContext dbContext)
    : BaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository
    {
        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User) // Загружаем связанного юзера сразу
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.JwtId == jwtId, cancellationToken);
        }
    }
}
