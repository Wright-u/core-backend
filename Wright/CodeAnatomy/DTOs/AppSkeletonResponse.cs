namespace Wright.CodeAnatomy.DTOs;

public class AppSkeletonResponse
{
    public List<FileSkeleton> Files { get; set; } = [];
}

public class FileSkeleton
{
    public required string Path { get; set; }
    public List<SignatureSkeleton> Signatures { get; set; } = [];
}

public class SignatureSkeleton
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Datatype { get; set; }
    public List<string> Modifiers { get; set; } = [];
    public List<SignatureSkeleton> Internals { get; set; } = [];
}
