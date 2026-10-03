using TreeSitter;
using Wright.Entities;

namespace Wright.AstTranslation.Interfaces;

public interface IAstTranslator
{
    Entity Translate(Node tree);
}