using TreeSitter;
using Wright.AstTranslation.Interfaces;
using Wright.Entities;

namespace Wright.AstTranslation.Translators;

public abstract class SingleNodeParser : IAstTranslator
{
    public abstract Entity? Translate(Node tree);
}