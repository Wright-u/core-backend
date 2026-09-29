namespace Wright.CodeAnatomy.DTOs;

public class SymbolReferencesResponse
{
    public List<SymbolReference> References { get; set; } = [];
}

public class SymbolReference
{
    public required string Source { get; set; }
}