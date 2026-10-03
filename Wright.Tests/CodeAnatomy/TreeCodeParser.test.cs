using Wright.CodeAnatomy.Utils;

namespace Wright.Tests.CodeAnatomy;

public class TreeCodeParserTest
{
    [Fact]
    public void ParseWhenNoLanguageThenThrowException()
    {
        TreeCodeParser parser = new();
        Assert.Throws<ArgumentNullException>(() => parser.Parse("", ""));
    }

    [Fact]
    public void ParseWhenNoCodeThenThrowException()
    {
        TreeCodeParser parser = new();
        Assert.Throws<ArgumentNullException>(() => parser.Parse("", "C#"));
    }

    [Fact]
    public void ParseWhenGoCodeThenReturnValidTree()
    {
        TreeCodeParser parser = new();
        var tree = parser.Parse("package main", "Go");
        Assert.NotNull(tree);
    }

    [Fact]
    public void ParseWhenLanguageIsUnsupportedThenIncludeLanguageAndOriginalException()
    {
        TreeCodeParser parser = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => parser.Parse("some code", "UnsupportedLanguage"));

        Assert.Contains("UnsupportedLanguage", exception.Message);
        Assert.NotNull(exception.InnerException);
    }
}