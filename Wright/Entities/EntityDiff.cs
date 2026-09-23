namespace Wright.Entities;

public class EntityDiff
{
    public List<string>? Mismatches { get; set; }
    public bool IsDifferent { get; set; }
}