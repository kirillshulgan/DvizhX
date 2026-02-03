namespace DvizhX.Application.Common.Interfaces.Authentication
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Генерирует JWT (Access Token).
        /// </summary>
        /// <returns>Кортеж: (Строка токена, Уникальный ID токена (JTI))</returns>
        (string AccessToken, string Jti) GenerateAccessToken(Guid userId, string username, string email);

        /// <summary>
        /// Генерирует криптографически стойкую случайную строку для Refresh Token.
        /// </summary>
        string GenerateRefreshToken();
    }
}
