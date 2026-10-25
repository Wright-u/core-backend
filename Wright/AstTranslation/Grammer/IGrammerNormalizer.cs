using Wright.Shared.Enums;

namespace Wright.AstTranslation.Grammer;

public interface IGrammerNormalizer
{
    IdentifierType? TryNormalize(string nodeType);
}
