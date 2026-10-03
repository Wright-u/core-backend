namespace Wright.Entities.Features.Function;

public class FunctionEntity : Entity
{
    public List<FunctionParameter> Parameters { get; set; } = [];
    public override EntityDiff Compare(Entity other)
    {
        if (other is not FunctionEntity entity) return new EntityDiff { IsDifferent = false, Mismatches = ["Not a FunctionEntity"] };

        if (Parameters.Count != entity.Parameters.Count) return new EntityDiff { IsDifferent = true, Mismatches = ["Parameter count mismatch"] };

        var mismatches = new List<string>();

        for (int i = 0; i < Parameters.Count; i++)
        {
            var parameter = Parameters[i];
            var otherParameter = entity.Parameters[i];
            if (parameter.Name != otherParameter.Name) 
                mismatches.Add($"Parameter {i} name mismatch");
                
            if (parameter.Type != otherParameter.Type) 
                mismatches.Add($"Parameter {i} type mismatch");
        }

        return new EntityDiff { IsDifferent = mismatches.Count > 0, Mismatches = mismatches };
    }
}

public class FunctionParameter
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}