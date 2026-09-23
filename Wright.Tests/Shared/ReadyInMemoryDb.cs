using Microsoft.EntityFrameworkCore;
using Wright.Contexts;

namespace Wright.Tests.Shared;

public class ReadyInMemoryDb
{
    public static DbContext Get()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }
}