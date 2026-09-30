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

    public Task<ImplementationResponse> GetImplementation(string source)
    {
        throw new NotImplementedException();
    }

    public Task<SymbolReferencesResponse> GetSymbolReferences(string source)
    {
        throw new NotImplementedException();
    }
}
