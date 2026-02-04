namespace DvizhX.Application.Features.Auth.Common
{
    public record TelegramAuthDto
    {
        public long Id { get; init; }
        public string? First_name { get; init; }
        public string? Last_name { get; init; }
        public string? Username { get; init; }
        public string? Photo_url { get; init; }
        public string Auth_date { get; init; } = string.Empty;
        public string Hash { get; init; } = string.Empty;
    }
}
