using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wright.Shared.Interfaces;

namespace Wright.Entities.Features.Function;

public class FunctionEntityDbConfig : IDbConfig<FunctionEntity>
{
    public void Configure(EntityTypeBuilder<FunctionEntity> builder)
    {
        builder.ToTable("Functions");
    }
}