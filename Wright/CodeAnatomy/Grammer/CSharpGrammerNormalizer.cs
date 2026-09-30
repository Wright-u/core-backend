using TreeSitter;
using Wright.CodeAnatomy.DTOs;

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
            ["local_function_statement"] = "function",
            ["parameter"] = "parameter",
        };

    public override string Language => "C#";

    protected override IReadOnlyDictionary<string, string> NodeTypes => CSharpNodeTypes;

    public override IEnumerable<Node> GetInternalNodes(Node node, SignatureSkeleton signature)
    {
        if (signature.Type is not ("method" or "function" or "constructor" or "destructor"))
        {
            return base.GetInternalNodes(node, signature);
        }

        Node? parameters = node.GetChildForField("parameters")
            ?? node.NamedChildren.FirstOrDefault(child => child.Type == "parameter_list");

        return parameters?.NamedChildren.Where(child => child.Type == "parameter") ?? [];
    }

    protected override string GetDatatype(Node node)
    {
        string datatype = base.GetDatatype(node);
        if (!string.IsNullOrWhiteSpace(datatype))
        {
            return datatype;
        }

        if (node.Type is not ("method_declaration" or "local_function_statement" or "parameter"))
        {
            return string.Empty;
        }

        Node? datatypeNode = node.NamedChildren.FirstOrDefault(child =>
            child.Type is "predefined_type" or "identifier" or "generic_name" or "array_type" or "nullable_type");

        return datatypeNode?.Text ?? string.Empty;
    }
}
