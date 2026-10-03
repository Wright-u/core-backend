using TreeSitter;
using Wright.AstTranslation.Interfaces;
using Wright.Entities;
using Wright.Entities.Features.Function;

namespace Wright.AstTranslation.Translators;

public class CTranslator : IAstTranslator
{
    public Entity Translate(Node tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        return tree.Type switch
        {
            "function_definition" => TranslateFunction(tree),
            _ => throw new NotSupportedException(
                $"C node type '{tree.Type}' cannot be translated to an entity."),
        };
    }

    private static FunctionEntity TranslateFunction(Node node)
    {
        Node? declarator = node.GetChildForField("declarator");
        string? name = declarator is null ? null : GetDeclaratorName(declarator);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "A C function definition does not contain a named declarator.");
        }

        Node? parameters = FindFunctionDeclarator(declarator)?.GetChildForField("parameters");

        return new FunctionEntity
        {
            Name = name,
            Parameters = parameters?.NamedChildren
                .Where(parameter => parameter.Type == "parameter_declaration")
                .Where(parameter => !IsVoidParameter(parameter))
                .Select(TranslateParameter)
                .ToList() ?? [],
        };
    }

    private static bool IsVoidParameter(Node parameter) =>
        parameter.GetChildForField("declarator") is null &&
        string.Equals(parameter.GetChildForField("type")?.Text, "void", StringComparison.Ordinal);

    private static FunctionParameter TranslateParameter(Node parameter)
    {
        Node? declarator = parameter.GetChildForField("declarator");
        string? name = declarator is null ? null : GetDeclaratorName(declarator);
        Node? type = parameter.GetChildForField("type");

        if (string.IsNullOrWhiteSpace(name) || type is null || string.IsNullOrWhiteSpace(type.Text))
        {
            throw new InvalidOperationException(
                $"The C parameter '{parameter.Text}' must have both a name and a type.");
        }

        return new FunctionParameter
        {
            Name = name,
            Type = type.Text,
        };
    }

    private static Node? FindFunctionDeclarator(Node? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Type == "function_declarator")
        {
            return node;
        }

        return node.GetChildForField("declarator") is Node nestedDeclarator
            ? FindFunctionDeclarator(nestedDeclarator)
            : null;
    }

    private static string? GetDeclaratorName(Node node)
    {
        if (node.Type == "identifier")
        {
            return node.Text;
        }

        Node? nestedDeclarator = node.GetChildForField("declarator");
        return nestedDeclarator is null ? null : GetDeclaratorName(nestedDeclarator);
    }
}
