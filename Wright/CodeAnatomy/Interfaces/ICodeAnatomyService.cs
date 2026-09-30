using Wright.CodeAnatomy.DTOs;

namespace Wright.CodeAnatomy.interfaces;

public interface ICodeAnatomyService
{
    public Task<AppSkeletonResponse> GetCodeSkeleton(string source);
    public Task<ImplementationResponse> GetImplementation(string source);
    public Task<SymbolReferencesResponse> GetSymbolReferences(string source);
}
