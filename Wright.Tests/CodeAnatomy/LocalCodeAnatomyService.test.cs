using Wright.CodeAnatomy.DTOs;
using Wright.CodeAnatomy.Grammer;
using Wright.CodeAnatomy.Services;
using Wright.CodeAnatomy.Utils;

namespace Wright.Tests.CodeAnatomy;

public class LocalCodeAnatomyServiceTest
    : IDisposable
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "Wright.Tests", Guid.NewGuid().ToString("N"));

    public LocalCodeAnatomyServiceTest()
    {
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task GetCodeSkeletonWhenApplicationDoesNotExistThenThrowsDirectoryNotFoundException()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => CreateService().GetCodeSkeleton("missing-app"));
    }

    [Fact]
    public async Task GetCodeSkeletonWhenApplicationIsEmptyThenReturnsAnEmptyAppSkeleton()
    {
        CreateApplication("empty-app");

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("empty-app");

        Assert.NotNull(appSkeleton);
        Assert.Empty(appSkeleton.Files);
    }

    [Fact]
    public async Task GetCodeSkeletonWhenApplicationContainsFilesThenReturnsAnAppSkeletonForEveryFile()
    {
        string appDirectory = CreateApplication("sample-app");
        string readmeFile = CreateFile(appDirectory, "README.md", "# Sample app");
        string programFile = CreateFile(appDirectory, "src/Program.cs", "Console.WriteLine(\"Hello\");");

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("sample-app");

        Assert.Equal(2, appSkeleton.Files.Count);
        Assert.Contains(appSkeleton.Files, file => file.Path == "sample-app/README.md");
        Assert.Contains(appSkeleton.Files, file => file.Path == "sample-app/src/Program.cs");
    }

    [Fact]
    public async Task GetCodeSkeletonWhenApplicationContainsOneFileThenFileSkeletonContainsTheFilePathAndNoSignatures()
    {
        string appDirectory = CreateApplication("single-file-app");
        string _ = CreateFile(appDirectory, "src/Program.cs", "Console.WriteLine(\"Hello\");");

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("single-file-app");
        FileSkeleton fileSkeleton = Assert.Single(appSkeleton.Files);

        Assert.Equal("single-file-app/src/Program.cs", fileSkeleton.Path);
        Assert.NotNull(fileSkeleton.Signatures);
        Assert.Empty(fileSkeleton.Signatures);
    }

    [Fact]
    public async Task GetCodeSkeletonWhenFileContainsTopLevelSignaturesThenReturnsTheirNamesTypesAndModifiers()
    {
        string appDirectory = CreateApplication("signature-app");
        CreateFile(appDirectory, "Models.cs", """
            public class Customer
            {
            }

            internal struct Address
            {
            }

            public interface ICustomerRepository
            {
            }

            public enum CustomerStatus
            {
                Active
            }
            """);

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("signature-app");
        FileSkeleton fileSkeleton = Assert.Single(appSkeleton.Files);

        Assert.Collection(
            fileSkeleton.Signatures,
            signature => AssertSignature(signature, "Customer", "class", "public"),
            signature => AssertSignature(signature, "Address", "struct", "internal"),
            signature => AssertSignature(signature, "ICustomerRepository", "interface", "public"),
            signature => AssertSignature(signature, "CustomerStatus", "enum", "public"));
    }

    [Fact]
    public async Task GetCodeSkeletonWhenClassContainsMethodsThenAddsMethodsAsChildSignatures()
    {
        string appDirectory = CreateApplication("nested-signature-app");
        CreateFile(appDirectory, "CustomerService.cs", """
            public class CustomerService
            {
                public string GetCustomerName(int customerId)
                {
                    return "Ada";
                }

                private void ClearCache()
                {
                }
            }
            """);

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("nested-signature-app");
        FileSkeleton fileSkeleton = Assert.Single(appSkeleton.Files);
        SignatureSkeleton classSignature = Assert.Single(fileSkeleton.Signatures);

        AssertSignature(classSignature, "CustomerService", "class", "public");
        Assert.Collection(
            classSignature.Contracts,
            signature => AssertSignature(signature, "GetCustomerName", "method", "public"),
            signature => AssertSignature(signature, "ClearCache", "method", "private"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private LocalCodeAnatomyService CreateService() => new(
        CreateRootUri(),
        new TreeCodeParser(),
        new GrammerNormalizerFactory([new CSharpGrammerNormalizer()]));

    private string CreateApplication(string appName) =>
        Directory.CreateDirectory(Path.Combine(_testDirectory, appName)).FullName;

    private static string CreateFile(string applicationDirectory, string relativePath, string content)
    {
        string filePath = Path.Combine(applicationDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);

        return filePath;
    }

    private static void AssertSignature(
        SignatureSkeleton signature,
        string expectedName,
        string expectedType,
        params string[] expectedModifiers)
    {
        Assert.Equal(expectedName, signature.Name);
        Assert.Equal(expectedType, signature.Type);
        Assert.Equal(expectedModifiers, signature.Modifiers);
    }

    private Uri CreateRootUri() => new(_testDirectory + Path.DirectorySeparatorChar);
}
