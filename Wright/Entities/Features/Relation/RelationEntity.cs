namespace Wright.Entities.Features.Relation;

public class RelationEntity : Entity
{
    public Guid TargetId { get; set; }
    public Guid SourceId { get; set; }
    public Entity? Target { get; set; }
    public Entity? Source { get; set; }
    public RelationTypes Type { get; set; }

    public override EntityDiff Compare(Entity other)
    {
        if (other is not RelationEntity otherRelation)
        {
            return new EntityDiff { IsDifferent = false, Mismatches = ["Not a RelationEntity"] };
        }

        if (Type != otherRelation.Type)
        {
            return new EntityDiff { IsDifferent = true, Mismatches = [$"Relation from {Source?.Name} to {Target?.Name} type mismatch"] };
        }

        return new EntityDiff { IsDifferent = false, Mismatches = [] };
    }
}