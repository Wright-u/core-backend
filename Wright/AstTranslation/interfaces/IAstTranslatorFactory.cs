namespace Wright.AstTranslation.Interfaces;

public interface IAstTranslatorFactory
{
    IAstTranslator Create(string language);
}