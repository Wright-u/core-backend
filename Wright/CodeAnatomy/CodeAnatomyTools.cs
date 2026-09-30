using System.ComponentModel;
using ModelContextProtocol.Server;
using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.interfaces;

namespace Wright.CodeAnatomy;

[McpServerToolType]
public class CodeAnatomyTools
{
    private readonly ICodeAnatomyService _service;

    public CodeAnatomyTools(ICodeAnatomyService service)
    {
        _service = service;
    }

    [McpServerTool(Name = "getAppSkeleton")]
    [Description("Retrives all the files content signatures of a given app")]
    public async Task<AppSkeletonResponse> GetAppSkeleton(
        [Description("URL of the repository of the app")] string url)
    {
        return await _service.GetCodeSkeleton(new (url));
    }

    [McpServerTool(Name = "getImplementation")]
    [Description("Get the implementation body of a method")]
    public async Task<ImplementationResponse> GetImplementation(
        [Description("URL of the method in the app. Example: <repo_url>/<file_name>/<parent_container_if_any>/<method_name>")] string url)
    {
        return await _service.GetImplementation(new (url));
    }

    [McpServerTool(Name = "getSymbolReferences")]
    [Description("Get the symbol references of a method")]
    public async Task<SymbolReferencesResponse> GetSymbolReferences(
        [Description("URL of the method in the app. Example: <repo_url>/<file_name>/<parent_container_if_any>/<method_name>")] string url)
    {
        return await _service.GetSymbolReferences(new (url));
    }
}
