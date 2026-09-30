using Wright.CodeAnatomy.DTOs;

namespace Wright.CodeAnatomy.interfaces;

public interface ICodeAnatomyService
{
    public Task<AppSkeletonResponse> GetCodeSkeleton(Uri source);
    public Task<ImplementationResponse> GetImplementation(Uri source);
    public Task<SymbolReferencesResponse> GetSymbolReferences(Uri source);
}
