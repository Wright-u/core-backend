using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wright.Shared.Interfaces;

public interface IDbConfig<T> : IEntityTypeConfiguration<T> where T : class
{
    public new void Configure(EntityTypeBuilder<T> builder);
}