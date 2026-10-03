namespace Wright.Judge.DTOs;

public class JudgeResponse
{
    public bool ISMatch { get; set; }
    public int TotalViolations { get; set; }
    public List<ViolationDto> Violations { get; set; } = new();
}

public class ViolationDto
{
    public required string EntityName { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; } // "calls" or "uses"
}