using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TT_Website.Data;
using TT_Website.Models;
using TT_Website.Services;

namespace TT_Website.Tests;

public class CalendarEventServiceTests
{
    [Fact]
    public async Task GetUpcomingAsync_ReturnsOnlyEventsInsideFourteenDayWindow()
    {
        await using var fixture = await CalendarFixture.CreateAsync();
        var start = new DateTime(2026, 8, 7);
        fixture.Context.CalendarEvents.AddRange(
            Event("Heute", start.AddHours(18)),
            Event("Letzter Tag", start.AddDays(13).AddHours(20)),
            Event("Zu spät", start.AddDays(14)));
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.GetUpcomingAsync(start, start.AddDays(13));

        Assert.Equal(["Heute", "Letzter Tag"], result.Select(x => x.Title));
    }

    [Fact]
    public async Task GetNextAsync_ReturnsChronologicallyNextFutureEvent()
    {
        await using var fixture = await CalendarFixture.CreateAsync();
        var now = new DateTime(2026, 8, 7, 12, 0, 0);
        fixture.Context.CalendarEvents.AddRange(
            Event("Vergangen", now.AddMinutes(-1)),
            Event("Danach", now.AddDays(1)),
            Event("Als Nächstes", now.AddMinutes(30)));
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.GetNextAsync(now);

        Assert.NotNull(result);
        Assert.Equal("Als Nächstes", result.Title);
    }

    private static CalendarEvent Event(string title, DateTime date) => new()
    {
        Title = title,
        Description = "Beschreibung",
        EventDate = date
    };

    private sealed class CalendarFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public AppDbContext Context { get; }
        public CalendarEventService Service { get; }

        private CalendarFixture(SqliteConnection connection, AppDbContext context)
        {
            this.connection = connection;
            Context = context;
            Service = new CalendarEventService(context, new TestEnvironment());
        }

        public static async Task<CalendarFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new CalendarFixture(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
