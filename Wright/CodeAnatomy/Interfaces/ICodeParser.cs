using TreeSitter;

namespace Wright.CodeAnatomy.interfaces;

public interface ICodeParser
{
    public Tree Parse(string codeText, string language);
}