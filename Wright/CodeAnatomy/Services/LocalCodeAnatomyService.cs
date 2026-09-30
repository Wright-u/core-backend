using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.Grammer;
using Wright.CodeAnatomy.interfaces;
using Wright.CodeAnatomy.Utils;

namespace Wright.CodeAnatomy.Services;

public class LocalCodeAnatomyService : ICodeAnatomyService
{
    private readonly Uri _root;
    private readonly ICodeParser _codeParser;
    private readonly IGrammerNormalizerFactory _grammerNormalizerFactory;

    public LocalCodeAnatomyService(
        Uri root,
        ICodeParser codeParser,
        IGrammerNormalizerFactory grammerNormalizerFactory)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (!root.IsAbsoluteUri || !root.IsFile)
        {
            throw new ArgumentException("The root must be an absolute file URI.", nameof(root));
        }

        _root = root;
        _codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
        _grammerNormalizerFactory = grammerNormalizerFactory ?? throw new ArgumentNullException(nameof(grammerNormalizerFactory));
    }

    public async Task<AppSkeletonResponse> GetCodeSkeleton(string source)
    {
        Uri app = new(_root, source);

        // Read all files in the app directory
        AppSkeletonResponse response = new();

        foreach (var file in Directory.EnumerateFiles(app.LocalPath, "*", SearchOption.AllDirectories))
        {
            var fileSkeleton = new FileSkeleton {Path = Path.GetRelativePath(_root.LocalPath, file).Replace('\\', '/')};
            string language = LanguageDetector.InferLanguage(file);
            IGrammerNormalizer? normalizer = _grammerNormalizerFactory.Get(language);
            if (normalizer is not null)
            {
                string content = await File.ReadAllTextAsync(file);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var tree = _codeParser.Parse(content, language);
                    AddSignatures(tree.RootNode, fileSkeleton.Signatures, normalizer);
                }
            }

            response.Files.Add(fileSkeleton);
        }

        return response;
    }

    private static void AddSignatures(
        TreeSitter.Node node,
        List<SignatureSkeleton> signatures,
        IGrammerNormalizer normalizer)
    {
        foreach (TreeSitter.Node child in node.NamedChildren)
        {
            if (normalizer.TryNormalize(child, out SignatureSkeleton? signature) && signature is not null)
            {
                signatures.Add(signature);
                AddSignatures(child, signature.Contracts, normalizer);
                continue;
            }

            AddSignatures(child, signatures, normalizer);
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
            bool hasReference = GetDescendants(tree.RootNode)
                .Any(node => normalizer.IsSymbolReference(node) && node.Text == targetSymbol);

            if (hasReference)
            {
                references.Add(new SymbolReference
                {
                    Source = Path.GetRelativePath(_root.LocalPath, file).Replace('\\', '/'),
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

}
