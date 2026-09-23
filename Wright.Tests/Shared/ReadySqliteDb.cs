using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Wright.Contexts;

namespace Wright.Tests.Shared;

public static class ReadySqliteDb
{
    public static AppDbContext Get()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}