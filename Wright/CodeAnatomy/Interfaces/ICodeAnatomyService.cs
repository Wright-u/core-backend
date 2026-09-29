using Wright.CodeAnatomy.DTOs;

namespace Wright.CodeAnatomy.interfaces;

public interface ICodeAnatomyService
{
    public AppSkeletonResponse GetCodeSkeleton(Uri source);
    public ImplementationResponse GetImplementation(Uri source);
    public SymbolReferencesResponse GetSymbolReferences(Uri source);
}
