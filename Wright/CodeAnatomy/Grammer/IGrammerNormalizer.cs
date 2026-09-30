using TreeSitter;
using Wright.CodeAnatomy.DTOs;

namespace Wright.CodeAnatomy.Grammer;

/// <summary>
/// Translates Tree-sitter nodes from one programming language into generic signatures.
/// </summary>
public interface IGrammerNormalizer
{
    string Language { get; }

    bool TryNormalize(Node node, out SignatureSkeleton? signature);

    bool IsSymbolReference(Node node);
}
