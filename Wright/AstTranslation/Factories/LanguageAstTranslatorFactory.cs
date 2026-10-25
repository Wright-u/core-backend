using Wright.AstTranslation.Interfaces;
using Wright.AstTranslation.Translators;
using Wright.AstTranslation.Translators.Cpp;

namespace Wright.AstTranslation.Services;

public class LanguageAstTranslatorFactory : IAstTranslatorFactory<string>
{
    public IAstTranslator Create(string language)
    {
        switch (language.ToLower())
        {
            case "c":
                return new CTranslator();
            case "cpp":
                // return new CppTranslator();
            default:
                throw new NotSupportedException($"Language '{language}' is not supported.");
        }
    }
}