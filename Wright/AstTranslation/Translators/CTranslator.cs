using TreeSitter;
using Wright.AstTranslation.Interfaces;
using Wright.Entities;
using Wright.Entities.Features.Function;
using Wright.Entities.Features.Relation;

namespace Wright.AstTranslation.Translators;

public class CTranslator : IAstTranslator
{
    private readonly Dictionary<string, FunctionEntity> _translatedFunctions =
        new(StringComparer.Ordinal);

    public Entity Translate(Node tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        return tree.Type switch
        {
            "function_definition" => TranslateFunction(tree),
            "call_expression" => TranslateCall(tree),
            _ => throw new NotSupportedException(
                $"C node type '{tree.Type}' cannot be translated to an entity."),
        };
    }

    private FunctionEntity TranslateFunction(Node node)
    {
        FunctionEntity function = TranslateFunctionWithoutRelations(node);
        _translatedFunctions[function.Name] = function;

        foreach (string calledFunction in GetCalledFunctions(node))
        {
            RelationEntity relation = CreateUsesRelation(function, calledFunction);
            function.Children.Add(relation);
        }

        return function;
    }

    private RelationEntity TranslateCall(Node node)
    {
        string? calledFunction = GetCalledFunctionName(node);
        Node? containingFunction = GetContainingFunction(node);

        if (string.IsNullOrWhiteSpace(calledFunction) || containingFunction is null)
        {
            throw new InvalidOperationException(
                "A C function call must have a named callee and be declared inside a function.");
        }

        return CreateUsesRelation(GetTranslatedFunction(containingFunction), calledFunction);
    }

    private FunctionEntity GetTranslatedFunction(Node node)
    {
        FunctionEntity function = TranslateFunctionWithoutRelations(node);
        return _translatedFunctions.TryGetValue(function.Name, out FunctionEntity? translatedFunction)
            ? translatedFunction
            : _translatedFunctions[function.Name] = function;
    }

    private static FunctionEntity TranslateFunctionWithoutRelations(Node node)
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

    private RelationEntity CreateUsesRelation(FunctionEntity source, string targetName)
    {
        FunctionEntity target = GetRelationTarget(source, targetName);

        return new RelationEntity
        {
            Name = target.Name,
            Type = RelationTypes.Uses,
            Source = source,
            SourceId = source.Id,
            Target = target,
            TargetId = target.Id,
            Parent = source,
            ParentId = source.Id,
        };
    }

    private FunctionEntity GetRelationTarget(FunctionEntity source, string targetName)
    {
        if (string.Equals(source.Name, targetName, StringComparison.Ordinal))
        {
            return source;
        }

        // A target not seen in this file is represented by a name-only substitute.
        return _translatedFunctions.TryGetValue(targetName, out FunctionEntity? target)
            ? target
            : new FunctionEntity { Name = targetName };
    }

    private static IEnumerable<string> GetCalledFunctions(Node function) =>
        GetDescendants(function)
            .Where(node => node.Type == "call_expression")
            .Select(GetCalledFunctionName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct(StringComparer.Ordinal);

    private static string? GetCalledFunctionName(Node call) =>
        call.GetChildForField("function")?.Text;

    private static Node? GetContainingFunction(Node node)
    {
        for (Node? current = node.Parent; current is not null; current = current.Parent)
        {
            if (current.Type == "function_definition")
            {
                return current;
            }
        }

        return null;
    }

    private static IEnumerable<Node> GetDescendants(Node node)
    {
        foreach (Node child in node.NamedChildren)
        {
            yield return child;

            foreach (Node descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
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
