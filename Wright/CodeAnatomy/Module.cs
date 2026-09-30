using Wright.CodeAnatomy.interfaces;
using Wright.CodeAnatomy.Services;
using Wright.Shared.Interfaces;

namespace Wright.CodeAnatomy;

public class CodeAnatomyModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<ICodeAnatomyService, LocalCodeAnatomyService>();
    }
}