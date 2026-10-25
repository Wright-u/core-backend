namespace Wright.AstTranslation.Interfaces;

public interface IAstTranslatorFactory<T>
{
    IAstTranslator Create(T param);
}