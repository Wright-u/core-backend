namespace Wright.Shared.Interfaces;

public interface IModule
{
    void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    );

    virtual void Configure(
        WebApplication app
    )
    {
        // Default implementation does nothing
    }

}