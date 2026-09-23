using Microsoft.EntityFrameworkCore;
using Wright.Contexts;

namespace Wright.Tests.Shared;

public class TestDbContext : AppDbContext
{
    public TestDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FakeEntityDbConfig).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}