using Microsoft.EntityFrameworkCore;
using Wright.Entities;
using Wright.Tests.Shared;

namespace Wright.Tests.Entities;

public class EntityRepositoryTest
{
    [Fact]
    public async Task AddEntityWhenNoneExistsThenPass()
    {

        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        var entityRepository = new EntityRepository(context);
        await entityRepository.Add(entity);

        Assert.NotEmpty(await context.Set<Entity>().ToListAsync());

        var entityFromDb = await context.Set<Entity>().FirstOrDefaultAsync(e => e.Id == entity.Id);

        Assert.NotNull(entityFromDb);
        Assert.Equal(entity.Id, entityFromDb.Id);
        Assert.Equal(entity.Name, entityFromDb.Name);
    }

    [Fact]
    public async Task AddEntityWhenExistsThenFail()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        await Assert.ThrowsAsync<ArgumentException>(() => entityRepository.Add(entity));
    }

    [Fact]
    public async Task AddFakeEntityWhenInheritsFromEntityThenAddParentEntity()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new FakeEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity",
            Attribute = "Test Attribute"
        };

        var entityRepository = new EntityRepository(context);
        await entityRepository.Add(entity);

        Assert.NotEmpty(await context.Set<Entity>().ToListAsync());
    }

    [Fact]
    public async Task DeleteEntityWhenExistsThenPass()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        await entityRepository.Delete(entity.Id);

        Assert.Empty(await context.Set<Entity>().ToListAsync());
    }

    [Fact]
    public async Task DeleteEntityWhenNotExistsThenDoNothing()
    {
        var context = ReadyInMemoryDb.Get();

        var entityRepository = new EntityRepository(context);
        await entityRepository.Delete(Guid.NewGuid());
    }

    [Fact]
    public async Task UpdateEntityWhenExistsThenPass()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        await entityRepository.Update(entity.Id, entity);

        Assert.Equal(entity.Name, (await context.Set<Entity>().FirstOrDefaultAsync(e => e.Id == entity.Id))?.Name);
    }

    [Fact]
    public async Task UpdateEntityWhenNotExistsThenThrowException()
    {
        var context = ReadyInMemoryDb.Get();

        var entityRepository = new EntityRepository(context);
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            entityRepository.Update(Guid.NewGuid(), new StubEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test Entity"
            }
            )
        );
    }

    [Fact]
    public async Task GetEntityByIdWhenExistsThenPass()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        Assert.Equal(entity.Name, (await entityRepository.GetById(entity.Id))?.Name);
    }

    [Fact]
    public async Task GetEntityByEntitySetThenCastToFakeEntity()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new FakeEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity",
            Attribute = "Test Attribute"
        };

        await context.Set<FakeEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        var getResult = await entityRepository.GetById(entity.Id);

        Assert.NotNull(getResult);
        Assert.Equal(entity.Attribute, ((FakeEntity)getResult)?.Attribute);
    }

    [Fact]
    public async Task GetEntityByIdWhenNotExistsThenReturnNull()
    {
        var context = ReadyInMemoryDb.Get();

        var entityRepository = new EntityRepository(context);
        Assert.Null(await entityRepository.GetById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllEntitiesThenPass()
    {
        var context = ReadyInMemoryDb.Get();

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        Assert.Single((await entityRepository.GetAll()).ToList());
    }

    [Fact]
    public async Task GetRecursivelyWhenExistsThenPass()
    {
        var context = ReadySqliteDb.Get();

        var parent = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity"
        };

        var entity = new StubEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity",
            ParentId = parent.Id
        };

        await context.Set<Entity>().AddAsync(parent);
        await context.Set<Entity>().AddAsync(entity);
        await context.SaveChangesAsync();

        var entityRepository = new EntityRepository(context);
        // Check collection has 2 elements
        Assert.Equal(2, (await entityRepository.GetRecursively(parent.Id)).Count());
    }
}