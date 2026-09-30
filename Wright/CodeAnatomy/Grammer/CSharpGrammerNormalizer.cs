namespace Wright.CodeAnatomy.Grammer;

/// <summary>
/// Normalizes the declaration node names emitted by the Tree-sitter C# grammar.
/// </summary>
public sealed class CSharpGrammerNormalizer : GrammerNormalizer
{
    private static readonly IReadOnlyDictionary<string, string> CSharpNodeTypes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace_declaration"] = "namespace",
            ["file_scoped_namespace_declaration"] = "namespace",
            ["class_declaration"] = "class",
            ["struct_declaration"] = "struct",
            ["interface_declaration"] = "interface",
            ["enum_declaration"] = "enum",
            ["delegate_declaration"] = "delegate",
            ["record_declaration"] = "record",
            ["method_declaration"] = "method",
            ["constructor_declaration"] = "constructor",
            ["destructor_declaration"] = "destructor",
            ["property_declaration"] = "property",
            ["indexer_declaration"] = "indexer",
            ["event_declaration"] = "event",
            ["event_field_declaration"] = "event",
            ["field_declaration"] = "field",
            ["operator_declaration"] = "operator",
            ["conversion_operator_declaration"] = "conversion_operator",
            ["enum_member_declaration"] = "enum_member",
        };

    public override string Language => "C#";

    protected override IReadOnlyDictionary<string, string> NodeTypes => CSharpNodeTypes;
}
