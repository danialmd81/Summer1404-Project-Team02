using System.Text.Json.Serialization;

namespace ETL.API.DTOs;

public record class TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public required int AccessExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public required int RefreshExpiresIn { get; init; }

    [JsonPropertyName("id_token")]
    public required string IdToken { get; init; }
}
