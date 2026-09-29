namespace Wright.CodeAnatomy.interfaces;

public interface ICodeAnatomyService
{
    public string GetCodeSkeleton(Uri source);
    public string GetImplementation(Uri source);
    public string GetSymbolReferences(Uri source);
}
