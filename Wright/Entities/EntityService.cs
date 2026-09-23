namespace Wright.Entities;

public class EntityService : IEntityService
{
    private readonly IEntityRepository _entityRepository;

    public EntityService(IEntityRepository entityRepository)
    {
        _entityRepository = entityRepository;
    }

    public async Task<Entity> GetEntityTree(Guid rootId)
    {
        var treeList = await _entityRepository.GetRecursively(rootId);
        var root = EntityTreeBuilder.Build(treeList, rootId);
        return root;
    }
}