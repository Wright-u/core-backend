using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wright.Shared.Interfaces;

namespace Wright.Entities.Features.Class;

public class ClassEntityDbConfig : IDbConfig<ClassEntity>
{
    public void Configure(EntityTypeBuilder<ClassEntity> builder)
    {
        builder.ToTable("Classes");
    }
}