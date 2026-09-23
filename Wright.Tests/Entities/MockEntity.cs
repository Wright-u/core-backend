using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wright.Entities;
using Wright.Shared.Interfaces;

namespace Wright.Tests.Entities;

class StubEntity : Entity
{
    public override EntityDiff Compare(Entity other)
    {
        return new EntityDiff
        {
            IsDifferent = false,
        };
    }
}

class FakeEntity : Entity
{
    public required string Attribute { get; set; }
    public override EntityDiff Compare(Entity other)
    {
        throw new NotImplementedException();
    }
}

class StubEntityDbConfig : IDbConfig<StubEntity>
{
    public void Configure(EntityTypeBuilder<StubEntity> builder)
    {
        builder.ToTable("StubEntities");
    }
}

class FakeEntityConfig : IDbConfig<FakeEntity>
{
    public void Configure(EntityTypeBuilder<FakeEntity> builder)
    {
        builder.ToTable("FakeEntities");
    }
}