using TreeSitter;
using Wright.CodeAnatomy.interfaces;

namespace Wright.CodeAnatomy.Utils;

public class TreeCodeParser : ICodeParser
{
    public Tree Parse(string codeText, string language)
    {
        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(codeText)) throw new ArgumentNullException(nameof(language));

        try
        {
            var grammer = new Language(language);
            var parser = new Parser(grammer);

            return parser.Parse(codeText) ?? throw new InvalidOperationException("Failed to parse code");
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Failed to parse code as '{language}': {exception.Message}",
                exception);
        }
    }
}