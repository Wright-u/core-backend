using Wright.CodeAnatomy.Grammer;

namespace Wright.Tests.CodeAnatomy.Grammer;

public class GrammerNormalizerFactoryTest
{
    [Fact]
    public void GetWhenLanguageHasNormalizerThenReturnsMatchingNormalizer()
    {
        GrammerNormalizerFactory factory = new([new CSharpGrammerNormalizer()]);

        IGrammerNormalizer? normalizer = factory.Get("C#");

        Assert.IsType<CSharpGrammerNormalizer>(normalizer);
    }

    [Fact]
    public void GetWhenLanguageHasNoNormalizerThenReturnsNull()
    {
        GrammerNormalizerFactory factory = new([new CSharpGrammerNormalizer()]);

        IGrammerNormalizer? normalizer = factory.Get("Python");

        Assert.Null(normalizer);
    }
}
