namespace Wright.CodeAnatomy.DTOs;

public class AppSkeleton
{
    
}

public class FileSkeleton
{
    
}

public class ContractSkeleton {}

public class ComposedContract
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public List<string> Modifiers { get; set; } = [];
    public List<ContractSkeleton> Contracts { get; set; } = [];
}

