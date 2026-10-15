using Wright.Shared.Interfaces;

namespace Wright.Ingestion.GitHub;

public class GitHubIngestionModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<GitHubAppJwt>();
        services.AddHttpClient<GitHubInstallationTokenService>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json")
            );

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Wright Anatomy");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/api/dev/github/{installationId}", async (long installationId, GitHubInstallationTokenService service) =>
        {
            var token = await service.Create(installationId, CancellationToken.None);
            return Results.Ok(token);
        });
    }
}