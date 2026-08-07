using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TT_Website.Data;
using TT_Website.Models;
using TT_Website.Services;

namespace TT_Website.Tests;

public class GalleryServiceVisibilityTests
{
    [Fact]
    public async Task PublicArchive_ReturnsOnlyPublicGroups()
    {
        await using var fixture = await GalleryFixture.CreateAsync();

        var groups = await fixture.Service.GetPublicGroupsAsync();

        var group = Assert.Single(groups);
        Assert.Equal("Öffentlich", group.Name);
    }

    [Fact]
    public async Task MemberGallery_ReturnsPublicAndInternalGroups()
    {
        await using var fixture = await GalleryFixture.CreateAsync();

        var groups = await fixture.Service.GetMemberGroupsAsync();

        Assert.Equal(2, groups.Count);
        Assert.Contains(groups, x => x.Name == "Öffentlich");
        Assert.Contains(groups, x => x.Name == "Intern");
    }

    private sealed class GalleryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly AppDbContext context;
        private readonly string webRoot;
        public GalleryService Service { get; }

        private GalleryFixture(SqliteConnection connection, AppDbContext context, GalleryService service, string webRoot)
        {
            this.connection = connection;
            this.context = context;
            Service = service;
            this.webRoot = webRoot;
        }

        public static async Task<GalleryFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var webRoot = Path.Combine(Path.GetTempPath(), $"tt-gallery-tests-{Guid.NewGuid():N}");
            var uploadDirectory = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadDirectory);
            await File.WriteAllBytesAsync(Path.Combine(uploadDirectory, "public.jpg"), [1]);
            await File.WriteAllBytesAsync(Path.Combine(uploadDirectory, "internal.jpg"), [1]);

            context.GalleryGroups.AddRange(
                CreateGroup("Öffentlich", true, "/uploads/public.jpg"),
                CreateGroup("Intern", false, "/uploads/internal.jpg"));
            await context.SaveChangesAsync();

            var environment = new TestEnvironment { WebRootPath = webRoot };
            return new GalleryFixture(connection, context, new GalleryService(context, environment), webRoot);
        }

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
            if (Directory.Exists(webRoot)) Directory.Delete(webRoot, recursive: true);
        }

        private static GalleryGroup CreateGroup(string name, bool isPublic, string path) => new()
        {
            Name = name,
            IsPublic = isPublic,
            Images =
            [
                new GalleryGroupImage
                {
                    GalleryImage = new GalleryImage { Title = name, ImagePath = path }
                }
            ]
        };
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
