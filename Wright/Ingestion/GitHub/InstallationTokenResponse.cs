using System.Text.Json.Serialization;

namespace Wright.Ingestion.GitHub;

public class InstallationTokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
    [JsonPropertyName("expires_at")]
    public required DateTimeOffset ExpiresAt { get; init; }
    [JsonPropertyName("permissions")]
    public Dictionary<string, string>? Permissions { get; init; }
}