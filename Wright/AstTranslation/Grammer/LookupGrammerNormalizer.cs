using Wright.Shared.Enums;

namespace Wright.AstTranslation.Grammer;

public abstract class LookupGrammerNormalizer : IGrammerNormalizer
{
    protected abstract IReadOnlyDictionary<string, IdentifierType> LookupTable { get; }

    public IdentifierType? TryNormalize(string nodeType)
    {
        if (LookupTable.ContainsKey(nodeType)) return LookupTable[nodeType];
        return null;
    }
}