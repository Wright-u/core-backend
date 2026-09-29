using TreeSitter;

namespace Wright.CodeAnatomy.interfaces;

public interface ICodeParser
{
    public Node Parse(string codeText);
}