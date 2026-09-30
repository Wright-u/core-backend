namespace Wright.CodeAnatomy.DTOs;

public class SymbolReferencesResponse
{
    public List<SymbolReference> References { get; set; } = [];
}

public class SymbolReference
{
    public required string Source { get; set; }
    public required SignatureSkeleton Signature { get; set; }
    public int LineNumber { get; set; }
}
