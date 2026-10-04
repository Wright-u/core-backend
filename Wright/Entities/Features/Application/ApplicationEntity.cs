namespace Wright.Entities.Features.Application;

public class ApplicationEntity : Entity
{
    public override EntityDiff Compare(Entity other)
    {
        return new EntityDiff
        {
            IsDifferent = other is not ApplicationEntity,
            Mismatches = other is ApplicationEntity ? [] : ["Not an ApplicationEntity"]
        };
    }
}
