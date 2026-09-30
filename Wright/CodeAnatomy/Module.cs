using Wright.CodeAnatomy.interfaces;
using Wright.CodeAnatomy.Grammer;
using Wright.CodeAnatomy.Services;
using Wright.CodeAnatomy.Utils;
using Wright.Shared.Interfaces;

namespace Wright.CodeAnatomy;

public class CodeAnatomyModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton<ICodeParser, TreeCodeParser>();
        services.AddSingleton<IGrammerNormalizer, CSharpGrammerNormalizer>();
        services.AddSingleton<IGrammerNormalizerFactory, GrammerNormalizerFactory>();
        services.AddScoped<ICodeAnatomyService, LocalCodeAnatomyService>();
    }
}
