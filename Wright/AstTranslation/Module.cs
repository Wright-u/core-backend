using Wright.AstTranslation.Interfaces;
using Wright.AstTranslation.Services;
using Wright.Shared.Interfaces;

namespace Wright.AstTranslation;

public class AstTranslationModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IAstTranslatorFactory, LanguageAstTranslatorFactory>();
    }
}