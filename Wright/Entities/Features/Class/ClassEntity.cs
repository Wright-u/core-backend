namespace Wright.Entities.Features.Class;

public class ClassEntity : Entity
{
    public List<AttributeField> Attributes { get; set; } = [];

    public override EntityDiff Compare(Entity other)
    {
        if (other is not ClassEntity entity) return new EntityDiff { IsDifferent = false, Mismatches = ["Not a ClassEntity"] };
        if (Attributes.Count != entity.Attributes.Count) return new EntityDiff { IsDifferent = true, Mismatches = ["Attribute count mismatch"] };

        for (int i = 0; i < Attributes.Count; i++)
        {
            var attribute = Attributes[i];
            var otherAttribute = entity.Attributes[i];
            if (attribute.Name != otherAttribute.Name) return new EntityDiff { IsDifferent = true, Mismatches = ["Attribute name mismatch"] };
            if (attribute.Type != otherAttribute.Type) return new EntityDiff { IsDifferent = true, Mismatches = ["Attribute type mismatch"] };
        }

        return new EntityDiff { IsDifferent = false, Mismatches = [] };
    }
}

public class AttributeField
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}