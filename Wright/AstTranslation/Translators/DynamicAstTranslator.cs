using TreeSitter;
using Wright.AstTranslation.Grammer;
using Wright.AstTranslation.Interfaces;
using Wright.Entities;
using Wright.Shared.Enums;

namespace Wright.AstTranslation.Translators;

public class DynamicAstTranslator : IAstTranslator
{
    private readonly IAstTranslatorFactory<IdentifierType> _translationFactory;
    private readonly IGrammerNormalizer _grammerNormalizer;

    public DynamicAstTranslator(
        IAstTranslatorFactory<IdentifierType> translationFactory,
        IGrammerNormalizer grammerNormalizer
        )
    {
        _translationFactory = translationFactory;
        _grammerNormalizer = grammerNormalizer;
    }

    public Entity? Translate(Node tree)
    {
        Entity? root = null;
        Stack<(Node Node, Entity? Parent)> nodes = new();
        nodes.Push((tree, null));

        while (nodes.Count > 0)
        {
            var (node, parent) = nodes.Pop();

            var nodeType = _grammerNormalizer.TryNormalize(node.Type);
            if (nodeType is null)
            {
                continue;
            }

            var translator = _translationFactory.Create((IdentifierType)nodeType);
            var entity = translator?.Translate(node);

            if (entity is not null)
            {
                if (parent is null) root ??= entity;
                else parent.Children.Add(entity);
            }

            var childParent = entity ?? parent;

            foreach (var child in node.NamedChildren.Reverse())
            {
                nodes.Push((child, childParent));
            }
        }

        return root;
    }
}