using Wright.AstTranslation.Interfaces;
using Wright.AstTranslation.Translators;

namespace Wright.AstTranslation.Services;

public class LanguageAstTranslatorFactory : IAstTranslatorFactory
{
    public IAstTranslator Create(string language)
    {
        switch (language.ToLower())
        {
            case "c":
                return new CTranslator();
            default:
                throw new NotSupportedException($"Language '{language}' is not supported.");
        }
    }
}