using ModelContextProtocol.Server;
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
}