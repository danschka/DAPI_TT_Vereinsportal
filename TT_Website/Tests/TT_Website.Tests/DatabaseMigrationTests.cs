using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TT_Website.Data;

namespace TT_Website.Tests;

public class DatabaseMigrationTests
{
    [Fact]
    public async Task AllMigrations_ApplyToEmptyDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new AppDbContext(options);

        await context.Database.MigrateAsync();

        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.False(await context.CalendarEvents.AnyAsync());
    }
}
