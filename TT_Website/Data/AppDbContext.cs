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
    public DbSet<DownloadDocument> DownloadDocuments => Set<DownloadDocument>();
    public DbSet<MemberApplication> MemberApplications => Set<MemberApplication>();
}