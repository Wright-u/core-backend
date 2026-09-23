using Microsoft.EntityFrameworkCore;

namespace Wright.Entities;

public class EntityRepository : IEntityRepository
{
    private readonly DbSet<Entity> _set;
    private readonly DbContext _context;

    public EntityRepository(DbContext context)
    {
        _set = context.Set<Entity>();
        _context = context;
    }

    public async Task Add(Entity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        try
        {
            await _set.Where(e => e.Id == id).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            var entity = await _set.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null) return;
            _set.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Entity>> GetAll()
    {
        return await _set.ToListAsync();
    }

    public async Task<Entity?> GetById(Guid id)
    {
        return await _set.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Entity>> GetRecursively(Guid rootId)
    {
        var ids = await _context.Database.SqlQuery<Guid>
        ($"""
            WITH RECURSIVE entity_tree AS (
                SELECT "Id" FROM "Entities" WHERE "Id" = {rootId}

                UNION ALL

                SELECT e."Id" FROM "Entities" e
                INNER JOIN entity_tree parent
                ON e."ParentId" = parent."Id"
            )
            SELECT "Id" FROM entity_tree
        """)
        .ToListAsync();

        return await _set.Where(e => ids.Contains(e.Id)).AsNoTracking().ToListAsync();
    }

    public async Task Update(Guid id, Entity entity)
    {
        var existing = await _set.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null) throw new KeyNotFoundException();

        _set.Update(entity);
        await _context.SaveChangesAsync();
    }
}