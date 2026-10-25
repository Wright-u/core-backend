using Wright.Shared.Enums;

namespace Wright.AstTranslation.Grammer;

public sealed class CSharpGrammerNormalizer : LookupGrammerNormalizer
{
    private readonly IReadOnlyDictionary<string, IdentifierType> _table =
        new Dictionary<string, IdentifierType>(StringComparer.Ordinal)
        {
            ["class_declaration"] = IdentifierType.CLASS,
            ["struct_declaration"] = IdentifierType.CLASS,
            ["constructor_declaration"] = IdentifierType.FUNCTION,
            ["destructor_declaration"] = IdentifierType.FUNCTION,
            ["local_function_statement"] = IdentifierType.FUNCTION,
        };

    protected override IReadOnlyDictionary<string, IdentifierType> LookupTable => _table;
}
