using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DvizhX.Infrastructure.Persistence.Repositories
{
    public class DeviceTokenRepository(ApplicationDbContext dbContext) : BaseRepository<DeviceToken>(dbContext), IDeviceTokenRepository
    {
        public async Task AddOrUpdateAsync(DeviceToken token, CancellationToken cancellationToken = default)
        {
            // 1. Проверяем, есть ли уже такой токен в БД
            var existingToken = await dbContext.DeviceTokens
                .FirstOrDefaultAsync(t => t.Token == token.Token, cancellationToken);

            if (existingToken != null)
            {
                // Если токен уже есть, обновляем время последнего использования
                // и привязываем к текущему юзеру (на случай смены аккаунта на устройстве)
                existingToken.LastUsed = DateTime.UtcNow;
                existingToken.UserId = token.UserId;
                existingToken.DeviceType = token.DeviceType;

                dbContext.DeviceTokens.Update(existingToken);
            }
            else
            {
                // Если нет — добавляем новый
                token.LastUsed = DateTime.UtcNow;
                await dbContext.DeviceTokens.AddAsync(token, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await dbContext.DeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.Token)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetTokensByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            // Используем Contains для выборки по списку ID
            return await dbContext.DeviceTokens
                .Where(t => userIds.Contains(t.UserId))
                .Select(t => t.Token)
                .Distinct() // Убираем дубликаты, если у юзера несколько устройств с одним токеном (маловероятно, но надежно)
                .ToListAsync(cancellationToken);
        }

        public async Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var entity = await dbContext.DeviceTokens
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

            if (entity != null)
            {
                dbContext.DeviceTokens.Remove(entity);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveAllTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await dbContext.DeviceTokens
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);

            if (tokens.Any())
            {
                dbContext.DeviceTokens.RemoveRange(tokens);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
