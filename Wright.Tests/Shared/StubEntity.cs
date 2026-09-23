using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wright.Entities;
using Wright.Shared.Interfaces;

namespace Wright.Tests.Shared;

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

class FakeEntityDbConfig : IDbConfig<StubEntity>
{
    public void Configure(EntityTypeBuilder<StubEntity> builder)
    {
        builder.ToTable("FakeEntities");
    }
}