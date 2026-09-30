namespace Wright.CodeAnatomy.Grammer;

public interface IGrammerNormalizerFactory
{
    IGrammerNormalizer? Get(string language);
}
