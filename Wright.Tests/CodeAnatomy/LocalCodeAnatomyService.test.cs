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
            signature => AssertSignature(signature, "Customer", "class", "", "public"),
            signature => AssertSignature(signature, "Address", "struct", "", "internal"),
            signature => AssertSignature(signature, "ICustomerRepository", "interface", "", "public"),
            signature => AssertSignature(signature, "CustomerStatus", "enum", "", "public"));
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

        AssertSignature(classSignature, "CustomerService", "class", "", "public");
        Assert.Collection(
            classSignature.Internals,
            signature => AssertSignature(signature, "GetCustomerName", "method", "string", "public"),
            signature => AssertSignature(signature, "ClearCache", "method", "void", "private"));
    }

    [Fact]
    public async Task GetCodeSkeletonWhenFileContainsGlobalFunctionThenReturnsFunctionWithoutItsBody()
    {
        string appDirectory = CreateApplication("global-function-app");
        CreateFile(appDirectory, "Program.cs", """
            void PrintGreeting(string name)
            {
                Console.WriteLine(name);
            }
            """);

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("global-function-app");
        FileSkeleton fileSkeleton = Assert.Single(appSkeleton.Files);
        SignatureSkeleton function = Assert.Single(fileSkeleton.Signatures);

        AssertSignature(function, "PrintGreeting", "function", "void");
        SignatureSkeleton parameter = Assert.Single(function.Internals);
        AssertSignature(parameter, "name", "parameter", "string");
    }

    [Fact]
    public async Task GetCodeSkeletonWhenMethodHasParametersThenAddsParametersAsInternals()
    {
        string appDirectory = CreateApplication("parameter-app");
        CreateFile(appDirectory, "Calculator.cs", """
            public class Calculator
            {
                public decimal Add(int left, decimal right)
                {
                    return left + right;
                }
            }
            """);

        AppSkeletonResponse appSkeleton = await CreateService().GetCodeSkeleton("parameter-app");
        SignatureSkeleton method = Assert.Single(Assert.Single(appSkeleton.Files).Signatures).Internals.Single();

        AssertSignature(method, "Add", "method", "decimal", "public");
        Assert.Collection(
            method.Internals,
            parameter => AssertSignature(parameter, "left", "parameter", "int"),
            parameter => AssertSignature(parameter, "right", "parameter", "decimal"));
    }

    [Fact]
    public async Task GetImplementationWhenMethodIsSpecifiedThenReturnsItsBodyStatements()
    {
        string appDirectory = CreateApplication("implementation-app");
        CreateFile(appDirectory, "CustomerService.cs", """
            public class CustomerService
            {
                public string GetCustomerName(int customerId)
                {
                    return "Ada";
                }
            }
            """);

        ImplementationResponse implementation = await CreateService()
            .GetImplementation("implementation-app/CustomerService.cs/CustomerService/GetCustomerName");

        Assert.Equal(["return \"Ada\";"], implementation.Body);
    }

    [Fact]
    public async Task GetImplementationWhenMethodIsNestedThenUsesTheFullContainerPath()
    {
        string appDirectory = CreateApplication("nested-implementation-app");
        CreateFile(appDirectory, "Container.cs", """
            public class Container
            {
                private class Worker
                {
                    public void Run()
                    {
                        Process();
                    }
                }
            }
            """);

        ImplementationResponse implementation = await CreateService()
            .GetImplementation("nested-implementation-app/Container.cs/Container/Worker/Run");

        Assert.Equal(["Process();"], implementation.Body);
    }

    [Fact]
    public async Task GetSymbolReferencesWhenMethodIsUsedInAnotherFileThenReturnsTheReferencingSource()
    {
        string appDirectory = CreateApplication("references-app");
        CreateFile(appDirectory, "CustomerService.cs", """
            public class CustomerService
            {
                public string GetCustomerName(int customerId)
                {
                    return repository.Find(customerId);
                }
            }
            """);
        CreateFile(appDirectory, "CustomerController.cs", """
            public class CustomerController
            {
                public string GetName(CustomerService service, int customerId)
                {
                    return service.GetCustomerName(customerId);
                }
            }
            """);

        SymbolReferencesResponse references = await CreateService()
            .GetSymbolReferences("references-app/CustomerService.cs/CustomerService/GetCustomerName");

        SymbolReference reference = Assert.Single(references.References);
        Assert.Equal("references-app/CustomerController.cs", reference.Source);
        Assert.Equal(5, reference.LineNumber);
        AssertSignature(reference.Signature, "GetName", "method", "string", "public");
    }

    [Fact]
    public async Task GetSymbolReferencesWhenNestedMethodIsUsedInAnotherFileThenReturnsTheReferencingSource()
    {
        string appDirectory = CreateApplication("nested-references-app");
        CreateFile(appDirectory, "Container.cs", """
            public class Container
            {
                public class Worker
                {
                    public void Run()
                    {
                        processor.Process(jobId);
                    }
                }
            }
            """);
        CreateFile(appDirectory, "Runner.cs", """
            public class Runner
            {
                public void Execute(Container.Worker worker)
                {
                    worker.Run();
                }
            }
            """);

        SymbolReferencesResponse references = await CreateService()
            .GetSymbolReferences("nested-references-app/Container.cs/Container/Worker/Run");

        SymbolReference reference = Assert.Single(references.References);
        Assert.Equal("nested-references-app/Runner.cs", reference.Source);
        Assert.Equal(5, reference.LineNumber);
        AssertSignature(reference.Signature, "Execute", "method", "void", "public");
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
        string expectedDatatype = "",
        params string[] expectedModifiers)
    {
        Assert.Equal(expectedName, signature.Name);
        Assert.Equal(expectedType, signature.Type);
        Assert.Equal(expectedDatatype, signature.Datatype);
        Assert.Equal(expectedModifiers, signature.Modifiers);
    }

    private Uri CreateRootUri() => new(_testDirectory + Path.DirectorySeparatorChar);
}
