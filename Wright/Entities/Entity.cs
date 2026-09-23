namespace Wright.Entities;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
    public Entity? Parent { get; set; }
    public List<Entity> Children { get; set; } = [];

    public abstract EntityDiff Compare(Entity other);
}