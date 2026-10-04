using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.Grammer;
using Wright.CodeAnatomy.interfaces;
using Wright.CodeAnatomy.Utils;
using Wright.AstTranslation.Interfaces;
using Wright.Entities;
using Wright.Entities.Features.Function;
using Wright.Entities.Features.Relation;

namespace Wright.CodeAnatomy.Services;

public class LocalCodeAnatomyService : ICodeAnatomyService
{
    private readonly Uri _root;
    private readonly ICodeParser _codeParser;
    private readonly IGrammerNormalizerFactory _grammerNormalizerFactory;
    private readonly IAstTranslatorFactory _astTranslatorFactory;

    public LocalCodeAnatomyService(
        [FromKeyedServices("RepositoriesUri")] Uri root,
        ICodeParser codeParser,
        IGrammerNormalizerFactory grammerNormalizerFactory,
        IAstTranslatorFactory astTranslatorFactory)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (!root.IsAbsoluteUri || !root.IsFile)
        {
            throw new ArgumentException("The root must be an absolute file URI.", nameof(root));
        }

        _root = root;
        _codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
        _grammerNormalizerFactory = grammerNormalizerFactory ?? throw new ArgumentNullException(nameof(grammerNormalizerFactory));
        _astTranslatorFactory = astTranslatorFactory ?? throw new ArgumentNullException(nameof(astTranslatorFactory));
    }

    public async Task<AppSkeletonResponse> GetCodeSkeleton(string source)
    {
        var app = Path.Combine(_root.LocalPath, source);

        // Read all files in the app directory
        AppSkeletonResponse response = new();

        foreach (var file in Directory.EnumerateFiles(app, "*", SearchOption.AllDirectories))
        {
            var fileSkeleton = new FileSkeleton { Path = Path.GetRelativePath(_root.LocalPath, file).Replace('\\', '/') };
            string language = LanguageDetector.InferLanguage(file);
            IGrammerNormalizer? normalizer = _grammerNormalizerFactory.Get(language);
            if (normalizer is not null)
            {
                string content = await File.ReadAllTextAsync(file);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var tree = _codeParser.Parse(content, language);
                    AddSignatures(tree.RootNode, fileSkeleton.Signatures, normalizer, []);
                }
            }

            response.Files.Add(fileSkeleton);
        }

        return response;
    }

    private static void AddSignatures(
        TreeSitter.Node node,
        List<SignatureSkeleton> signatures,
        IGrammerNormalizer normalizer,
        IReadOnlyList<string> containingSymbolPath)
    {
        foreach (TreeSitter.Node child in node.NamedChildren)
        {
            AddSignature(child, signatures, normalizer, containingSymbolPath);
        }
    }

    private static void AddSignature(
        TreeSitter.Node node,
        List<SignatureSkeleton> signatures,
        IGrammerNormalizer normalizer,
        IReadOnlyList<string> containingSymbolPath)
    {
        if (normalizer.TryNormalize(node, out SignatureSkeleton? signature) && signature is not null)
        {
            List<string> symbolPath = signature.Type == "namespace"
                ? [.. containingSymbolPath]
                : [.. containingSymbolPath, signature.Name];
            signature.SymbolPath = symbolPath;
            signature.CanReadCode = signature.Type is "method" or "function" or "constructor" or "destructor";
            signatures.Add(signature);
            AddSignatureInternals(node, signature, normalizer, symbolPath);

            return;
        }

        AddSignatures(node, signatures, normalizer, containingSymbolPath);
    }

    private static void AddSignatureInternals(
        TreeSitter.Node node,
        SignatureSkeleton signature,
        IGrammerNormalizer normalizer,
        IReadOnlyList<string> symbolPath)
    {
        foreach (TreeSitter.Node internalNode in normalizer.GetInternalNodes(node, signature))
        {
            AddSignature(internalNode, signature.Internals, normalizer, symbolPath);
        }
    }

    public async Task<ImplementationResponse> GetImplementation(string source)
    {
        (TreeSitter.Node declaration, _) = await GetDeclaration(source);
        return new ImplementationResponse { Body = GetBodyStatements(declaration) };
    }

    public async Task<SymbolReferencesResponse> GetSymbolReferences(string source)
    {
        (string targetFilePath, string[] symbolPath) = ResolveImplementationSource(source);
        _ = await GetDeclaration(source);
        string targetSymbol = symbolPath[^1];
        List<SymbolReference> references = [];

        foreach (string file in Directory.EnumerateFiles(_root.LocalPath, "*", SearchOption.AllDirectories))
        {
            if (string.Equals(file, targetFilePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string language = LanguageDetector.InferLanguage(file);
            IGrammerNormalizer? normalizer = _grammerNormalizerFactory.Get(language);
            if (normalizer is null)
            {
                continue;
            }

            string content = await File.ReadAllTextAsync(file);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var tree = _codeParser.Parse(content, language);
            foreach (TreeSitter.Node referenceNode in GetDescendants(tree.RootNode)
                .Where(node => normalizer.IsSymbolReference(node) && node.Text == targetSymbol))
            {
                SignatureSkeleton? signature = GetEnclosingSignature(referenceNode, normalizer);
                if (signature is null)
                {
                    continue;
                }

                references.Add(new SymbolReference
                {
                    Source = Path.GetRelativePath(_root.LocalPath, file).Replace('\\', '/'),
                    Signature = signature,
                    LineNumber = referenceNode.StartPosition.Row + 1,
                });
            }
        }

        return new SymbolReferencesResponse { References = references };
    }

    private async Task<(TreeSitter.Node Declaration, IGrammerNormalizer Normalizer)> GetDeclaration(string source)
    {
        (string filePath, string[] symbolPath) = ResolveImplementationSource(source);
        string language = LanguageDetector.InferLanguage(filePath);
        IGrammerNormalizer? normalizer = _grammerNormalizerFactory.Get(language);

        if (normalizer is null)
        {
            throw new NotSupportedException($"No signature normalizer is registered for '{language}'.");
        }

        string content = await File.ReadAllTextAsync(filePath);
        var tree = _codeParser.Parse(content, language);
        TreeSitter.Node? declaration = FindDeclaration(tree.RootNode, symbolPath, 0, normalizer);

        return declaration is null
            ? throw new KeyNotFoundException($"Could not find the declaration '{string.Join('/', symbolPath)}'.")
            : (declaration, normalizer);
    }

    private (string FilePath, string[] SymbolPath) ResolveImplementationSource(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        string[] segments = source
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int fileSegmentCount = segments.Length; fileSegmentCount > 0; fileSegmentCount--)
        {
            string candidatePath = Path.GetFullPath(Path.Combine([_root.LocalPath, .. segments[..fileSegmentCount]]));
            if (File.Exists(candidatePath))
            {
                string[] symbolPath = segments[fileSegmentCount..];
                if (symbolPath.Length == 0)
                {
                    throw new ArgumentException("The source must include a declaration path after the file path.", nameof(source));
                }

                return (candidatePath, symbolPath);
            }
        }

        throw new FileNotFoundException($"Could not find a source file in '{source}'.", source);
    }

    private static TreeSitter.Node? FindDeclaration(
        TreeSitter.Node node,
        IReadOnlyList<string> symbolPath,
        int symbolIndex,
        IGrammerNormalizer normalizer)
    {
        foreach (TreeSitter.Node child in node.NamedChildren)
        {
            if (normalizer.TryNormalize(child, out SignatureSkeleton? signature) && signature is not null)
            {
                if (signature.Name == symbolPath[symbolIndex])
                {
                    if (symbolIndex == symbolPath.Count - 1)
                    {
                        return child;
                    }

                    TreeSitter.Node? nestedDeclaration = FindDeclaration(child, symbolPath, symbolIndex + 1, normalizer);
                    if (nestedDeclaration is not null)
                    {
                        return nestedDeclaration;
                    }
                }

                TreeSitter.Node? declarationInChild = FindDeclaration(child, symbolPath, symbolIndex, normalizer);
                if (declarationInChild is not null)
                {
                    return declarationInChild;
                }

                continue;
            }

            TreeSitter.Node? declarationInDescendant = FindDeclaration(child, symbolPath, symbolIndex, normalizer);
            if (declarationInDescendant is not null)
            {
                return declarationInDescendant;
            }
        }

        return null;
    }

    private static SignatureSkeleton? GetEnclosingSignature(
        TreeSitter.Node node,
        IGrammerNormalizer normalizer)
    {
        for (TreeSitter.Node? current = node.Parent; current is not null; current = current.Parent)
        {
            if (normalizer.TryNormalize(current, out SignatureSkeleton? signature) && signature is not null)
            {
                AddSignatureInternals(current, signature, normalizer, signature.SymbolPath);
                return signature;
            }
        }

        return null;
    }

    private static List<string> GetBodyStatements(TreeSitter.Node declaration)
    {
        TreeSitter.Node body = GetBody(declaration);

        if (body.Type != "block")
        {
            return [body.Text.Trim()];
        }

        return body.NamedChildren
            .Select(statement => statement.Text.Trim())
            .Where(statement => !string.IsNullOrWhiteSpace(statement))
            .ToList();
    }

    private static TreeSitter.Node GetBody(TreeSitter.Node declaration) =>
        declaration.GetChildForField("body")
        ?? throw new InvalidOperationException($"The declaration '{declaration.Text}' does not have an implementation body.");

    private static IEnumerable<TreeSitter.Node> GetDescendants(TreeSitter.Node node)
    {
        foreach (TreeSitter.Node child in node.NamedChildren)
        {
            yield return child;

            foreach (TreeSitter.Node descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    public async Task<List<Entity>> ParseCodeToEntities(string source)
    {
        var app = Path.Combine(_root.LocalPath, source);
        List<Entity> entities = [];

        foreach (string file in Directory.EnumerateFiles(app, "*", SearchOption.AllDirectories))
        {
            string language = LanguageDetector.InferLanguage(file);
            IAstTranslator translator;

            try
            {
                translator = _astTranslatorFactory.Create(language);
            }
            catch (NotSupportedException)
            {
                continue;
            }

            string content = await File.ReadAllTextAsync(file);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            TreeSitter.Tree tree = _codeParser.Parse(content, language);
            foreach (TreeSitter.Node node in tree.RootNode.NamedChildren)
            {
                try
                {
                    Entity entity = translator.Translate(node);
                    AddEntityTree(entity, entities);
                }
                catch (NotSupportedException)
                {
                    // The translator does not model every syntax node in a supported file.
                }
            }
        }

        ResolveRelationTargets(entities);
        return BuildEntityForest(entities);
    }

    private static List<Entity> BuildEntityForest(List<Entity> entities)
    {
        if (entities.Count == 0)
        {
            return [];
        }

        List<Entity> roots = entities.Where(entity => entity.ParentId is null).ToList();
        if (roots.Count == 0)
        {
            throw new InvalidOperationException("The translated entities do not contain a root entity.");
        }

        // Build processes the entire collection, including every root, in one pass.
        _ = EntityTreeBuilder.Build(entities, roots[0].Id);
        return roots;
    }

    private static void AddEntityTree(Entity entity, List<Entity> entities)
    {
        entities.Add(entity);

        foreach (Entity child in entity.Children)
        {
            child.Parent = entity;
            child.ParentId = entity.Id;
            AddEntityTree(child, entities);
        }
    }

    private static void ResolveRelationTargets(IEnumerable<Entity> entities)
    {
        List<FunctionEntity> functions = entities.OfType<FunctionEntity>().ToList();

        foreach (RelationEntity relation in entities.OfType<RelationEntity>())
        {
            FunctionEntity? target = functions.FirstOrDefault(function =>
                string.Equals(function.Name, relation.Target?.Name, StringComparison.Ordinal));

            if (target is not null)
            {
                relation.Target = target;
                relation.TargetId = target.Id;
            }
        }
    }
}
