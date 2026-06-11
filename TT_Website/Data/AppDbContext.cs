using Microsoft.EntityFrameworkCore;
using TT_Website.Models;

namespace TT_Website.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<GalleryGroup> GalleryGroups => Set<GalleryGroup>();
    public DbSet<GalleryGroupImage> GalleryGroupImages => Set<GalleryGroupImage>();
    public DbSet<PageGalleryAssignment> PageGalleryAssignments => Set<PageGalleryAssignment>();
    public DbSet<DownloadDocument> DownloadDocuments => Set<DownloadDocument>();
    public DbSet<MembershipApplication> MemberApplications => Set<MembershipApplication>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamRound> TeamRounds => Set<TeamRound>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();
    public DbSet<ContentPageGalleryGroup> ContentPageGalleryGroups => Set<ContentPageGalleryGroup>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GalleryGroupImage>()
            .HasIndex(x => new { x.GalleryGroupId, x.GalleryImageId })
            .IsUnique();

        modelBuilder.Entity<PageGalleryAssignment>()
            .HasIndex(x => x.PageKey)
            .IsUnique();

        modelBuilder.Entity<TeamRound>()
            .HasIndex(x => new { x.TeamId, x.RoundName })
            .IsUnique();

        modelBuilder.Entity<ContentPage>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        modelBuilder.Entity<ContentPage>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ContentPageGalleryGroup>()
            .HasIndex(x => new { x.ContentPageId, x.GalleryGroupId })
            .IsUnique();

        modelBuilder.Entity<SiteSetting>()
            .HasIndex(x => x.Key)
            .IsUnique();
    }
}
