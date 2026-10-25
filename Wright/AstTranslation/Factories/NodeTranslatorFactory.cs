using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Wright.AstTranslation.Interfaces;
using Wright.Shared.Enums;

namespace Wright.AstTranslation.Factories;

public class NodeTranslatorFactory : IAstTranslatorFactory<IdentifierType>
{
    public IAstTranslator Create(IdentifierType nodeType)
    {
        return null!;
    }
}