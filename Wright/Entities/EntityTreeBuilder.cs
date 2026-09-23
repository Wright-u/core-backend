namespace Wright.Entities;

public static class EntityTreeBuilder
{
    public static Entity Build(IEnumerable<Entity> entities, Guid rootId)
    {
        if (!entities.Any()) throw new ArgumentNullException("Empty collection.");

        var all = entities.ToList();
        var byId = all.ToDictionary(x => x.Id);

        foreach (var entity in all)
        {
            entity.Children.Clear();

            if (entity.ParentId is not Guid parentId)
                continue;

            if (!byId.TryGetValue(parentId, out var parent))
                throw new InvalidOperationException(
                    $"Parent {parentId} not found.");

            parent.Children.Add(entity);
            entity.Parent = parent;
        }

        var root = all.FirstOrDefault(x => x.Id == rootId);
        if (root is null)
            throw new InvalidOperationException($"Root {rootId} not found.");

        return root;
    }
}