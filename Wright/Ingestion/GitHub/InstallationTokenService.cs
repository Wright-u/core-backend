using System.Text.Json;

namespace Wright.Ingestion.GitHub;

public class GitHubInstallationTokenService
{
    private readonly HttpClient _http;
    private readonly GitHubAppJwt _tokenGenerator;

    public GitHubInstallationTokenService(HttpClient http, GitHubAppJwt tokenGenerator) { 
        (_http, _tokenGenerator) = (http, tokenGenerator);
    }

    public async Task<InstallationTokenResponse> Create(long installationId, CancellationToken cancellationToken)
    {
        var jwt = _tokenGenerator.Generate();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"app/installations/{installationId}/access_tokens");
        request.Headers.Add("Authorization", $"Bearer {jwt}");

        using var response = await _http.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"Failed to create installation token. Status code: {response.StatusCode}. Body: {responseBody}");
        }

        return JsonSerializer.Deserialize<InstallationTokenResponse>(responseBody) ?? throw new InvalidOperationException("Failed to deserialize response.");
    }
}
