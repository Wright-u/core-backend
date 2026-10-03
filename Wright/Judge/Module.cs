using Wright.Judge.Interfaces;
using Wright.Shared.Interfaces;

namespace Wright.Judge;

public class JudgeModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IJudgeService, JudgeService>();
    }
}