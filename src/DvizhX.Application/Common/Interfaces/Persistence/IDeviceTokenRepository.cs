using DvizhX.Domain.Entities;

namespace DvizhX.Application.Common.Interfaces.Persistence
{
    public interface IDeviceTokenRepository : IRepository<DeviceToken>
    {
        /// <summary>
        /// Добавить новый токен или обновить существующий (например, дату использования).
        /// </summary>
        Task AddOrUpdateAsync(DeviceToken token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все токены для конкретного пользователя.
        /// </summary>
        Task<List<string>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить токены для нескольких пользователей сразу (для массовой рассылки).
        /// </summary>
        Task<List<string>> GetTokensByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить конкретный токен (например, при логауте или ошибке "InvalidRegistration").
        /// </summary>
        Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить все токены пользователя (например, при удалении аккаунта).
        /// </summary>
        Task RemoveAllTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
