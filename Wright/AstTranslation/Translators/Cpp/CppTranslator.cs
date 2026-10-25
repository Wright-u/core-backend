using Wright.AstTranslation.Interfaces;
using Wright.Shared.Enums;

namespace Wright.AstTranslation.Translators.Cpp;

public class CppTranslator : DynamicAstTranslator
{
    public CppTranslator(IAstTranslatorFactory<IdentifierType> translationFactory) : base(translationFactory)
    {
    }
}