using TreeSitter;
using Wright.CodeAnatomy.DTOs;

namespace Wright.CodeAnatomy.Grammer;

public abstract class GrammerNormalizer : IGrammerNormalizer
{
    private static readonly HashSet<string> ModifierKeywords = new(StringComparer.Ordinal)
    {
        "public", "private", "protected", "internal", "static", "abstract", "virtual", "override",
        "sealed", "partial", "async", "extern", "unsafe", "new", "readonly", "volatile", "const",
        "final", "native", "synchronized", "transient", "strictfp", "export", "default",
    };

    public abstract string Language { get; }

    protected abstract IReadOnlyDictionary<string, string> NodeTypes { get; }

    public bool TryNormalize(Node node, out SignatureSkeleton? signature)
    {
        if (!NodeTypes.TryGetValue(node.Type, out string? type))
        {
            signature = null;
            return false;
        }

        string? name = GetName(node);
        if (string.IsNullOrWhiteSpace(name))
        {
            signature = null;
            return false;
        }

        signature = new SignatureSkeleton
        {
            Name = name,
            Type = type,
            Modifiers = node.Children
                .Select(child => child.Text)
                .Where(ModifierKeywords.Contains)
                .ToList(),
        };

        return true;
    }

    public virtual bool IsSymbolReference(Node node) => node.Type == "identifier";

    private static string? GetName(Node node)
    {
        Node? nameNode = node.GetChildForField("name");
        if (nameNode is not null)
        {
            return nameNode.Text;
        }

        Node? directName = node.NamedChildren.FirstOrDefault(child =>
            child.Type is "identifier" or "type_identifier" or "property_identifier" or "field_identifier");
        if (directName is not null)
        {
            return directName.Text;
        }

        Node? declarator = node.NamedChildren.FirstOrDefault(child =>
            child.Type is "variable_declarator" or "variable_declaration" or "variable_definition");
        return declarator is null ? null : GetName(declarator);
    }
}
