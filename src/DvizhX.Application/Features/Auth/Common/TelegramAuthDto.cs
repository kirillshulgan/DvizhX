using System.Text.Json.Serialization;

namespace DvizhX.Application.Features.Auth.Common
{
    public record TelegramAuthDto
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("username")]
        public string? Username { get; init; }

        [JsonPropertyName("photo_url")]
        public string? PhotoUrl { get; init; }

        [JsonPropertyName("auth_date")]
        public string AuthDate { get; init; } = string.Empty;

        [JsonPropertyName("hash")]
        public string Hash { get; init; } = string.Empty;
    }
}
