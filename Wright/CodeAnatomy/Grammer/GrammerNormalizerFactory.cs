namespace Wright.CodeAnatomy.Grammer;

/// <summary>
/// Selects the normalizer matching the language detected for a source file.
/// </summary>
public class GrammerNormalizerFactory : IGrammerNormalizerFactory
{
    private readonly IReadOnlyDictionary<string, IGrammerNormalizer> _normalizers;

    public GrammerNormalizerFactory(IEnumerable<IGrammerNormalizer> normalizers)
    {
        _normalizers = normalizers.ToDictionary(normalizer => normalizer.Language, StringComparer.OrdinalIgnoreCase);
    }

    public IGrammerNormalizer? Get(string language) =>
        _normalizers.GetValueOrDefault(language);
}
