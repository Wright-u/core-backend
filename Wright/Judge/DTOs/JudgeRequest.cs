namespace Wright.Judge.DTOs;

public class JudgeRequest
{
    public required string AppId { get; set; }
    public required DesignSchemaDto DesignSchema { get; set; }
}

public class DesignSchemaDto
{
    public List<ElementDto> Elements { get; set; } = [];
    public List<RelationshipDto> Relationships { get; set; } = [];
}

public class ElementDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }          // function or variable
    public List<PropertyDto> Properties { get; set; } = [];  // function parameters / struct fields
}

public class PropertyDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}

public class RelationshipDto
{
    public required string SourceId { get; set; }
    public required string TargetId { get; set; }
    public string? Type { get; set; }                  // call or use
}