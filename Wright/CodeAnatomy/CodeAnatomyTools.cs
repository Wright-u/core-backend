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

    [McpServerTool(Name = "getApplicationStructure", UseStructuredContent = true)]
    [Description("Lists an application's files and declared symbols. Call this first. Each signature's symbolPath is canonical and is the only path accepted by symbol tools; use readSymbolCode only when canReadCode is true.")]
    public async Task<AppSkeletonResponse> GetApplicationStructure(
        [Description("Application name.")] string appName)
    {
        return await ExecuteToolAsync(() => _service.GetCodeSkeleton(appName));
    }

    [McpServerTool(Name = "readSymbolCode", UseStructuredContent = true)]
    [Description("Reads the implementation body of a declared symbol. Supply filePath and symbolPath exactly as returned by getApplicationStructure, and only for a signature whose canReadCode is true.")]
    public async Task<ImplementationResponse> ReadSymbolCode(
        [Description("Application name. This scopes the request.")] string appName,
        [Description("A file path returned by getApplicationStructure.")] string filePath,
        [Description("Ordered symbol names from the file root to the target declaration.")] IReadOnlyList<string> symbolPath)
    {
        return await ExecuteToolAsync(() => _service.GetImplementation(BuildSource(appName, filePath, symbolPath)));
    }

    [McpServerTool(Name = "findSymbolReferences", UseStructuredContent = true)]
    [Description("Finds declarations in the application that reference a declared symbol. Supply filePath and symbolPath exactly as returned by getApplicationStructure.")]
    public async Task<SymbolReferencesResponse> FindSymbolReferences(
        [Description("Application name. This scopes the request.")] string appName,
        [Description("A file path returned by getApplicationStructure.")] string filePath,
        [Description("Ordered symbol names from the file root to the target declaration.")] IReadOnlyList<string> symbolPath)
    {
        return await ExecuteToolAsync(() => _service.GetSymbolReferences(BuildSource(appName, filePath, symbolPath)));
    }

    private static string BuildSource(string appName, string filePath, IReadOnlyList<string> symbolPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(symbolPath);

        string normalizedFilePath = filePath.Replace('\\', '/').TrimStart('/');
        string normalizedAppName = appName.Replace('\\', '/').Trim('/');
        if (!normalizedFilePath.StartsWith($"{normalizedAppName}/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("filePath must belong to appName and come from getApplicationStructure.", nameof(filePath));
        }

        if (symbolPath.Count == 0 || symbolPath.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("symbolPath must contain at least one symbol name.", nameof(symbolPath));
        }

        return string.Join('/', [normalizedFilePath, .. symbolPath]);
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
