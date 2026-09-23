namespace Wright.Entities;

public interface IEntityService
{
    public Task<Entity> GetEntityTree(Guid rootId);
}