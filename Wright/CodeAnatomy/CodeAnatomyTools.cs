using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.interfaces;

namespace Wright.CodeAnatomy;

[McpServerToolType]
public class CodeAnatomyTools
{
    private readonly ICodeAnatomyService _service;
    private readonly ILogger<CodeAnatomyTools> _logger;

    public CodeAnatomyTools(ICodeAnatomyService service, ILogger<CodeAnatomyTools> logger)
    {
        _service = service;
        _logger = logger;
    }

    [McpServerTool(Name = "getAppSkeleton", UseStructuredContent = true)]
    [Description("Retrives all the files content signatures of a given app")]
    public async Task<AppSkeletonResponse> GetAppSkeleton(
        [Description("URL of the repository of the app")] string url)
    {
        return await ExecuteToolAsync(() => _service.GetCodeSkeleton(url));
    }

    [McpServerTool(Name = "getImplementation", UseStructuredContent = true)]
    [Description("Get the implementation body of a method")]
    public async Task<ImplementationResponse> GetImplementation(
        [Description("URL of the method in the app. Example: <repo_url>/<file_name>/<parent_container_if_any>/<method_name>")] string url)
    {
        return await ExecuteToolAsync(() => _service.GetImplementation(url));
    }

    [McpServerTool(Name = "getSymbolReferences", UseStructuredContent = true)]
    [Description("Get the symbol references of a method")]
    public async Task<SymbolReferencesResponse> GetSymbolReferences(
        [Description("URL of the method in the app. Example: <repo_url>/<file_name>/<parent_container_if_any>/<method_name>")] string url)
    {
        return await ExecuteToolAsync(() => _service.GetSymbolReferences(url));
    }

    private async Task<T> ExecuteToolAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (McpException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Code anatomy MCP tool failed.");
            throw new McpException($"Code anatomy request failed: {exception.Message}");
        }
    }
}
