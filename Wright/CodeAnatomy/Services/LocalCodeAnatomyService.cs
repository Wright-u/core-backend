using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.interfaces;

namespace Wright.CodeAnatomy.Services;

public class LocalCodeAnatomyService : ICodeAnatomyService
{
    public Task<AppSkeletonResponse> GetCodeSkeleton(Uri source)
    {
        throw new NotImplementedException();
    }

    public Task<ImplementationResponse> GetImplementation(Uri source)
    {
        throw new NotImplementedException();
    }

    public Task<SymbolReferencesResponse> GetSymbolReferences(Uri source)
    {
        throw new NotImplementedException();
    }
}