namespace Wright.Entities;

public interface IEntityRepository
{
    public Task Add(Entity entity);
    public Task Update(Guid id, Entity entity);
    public Task<Entity?> GetById(Guid id);
    public Task<IEnumerable<Entity>> GetRecursively(Guid rootId);
    public Task<IEnumerable<Entity>> GetAll();
    public Task Delete(Guid id);
}