using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class SponsorService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public SponsorService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<List<Sponsor>> GetAllAsync()
    {
        var sponsors = await _context.Sponsors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        ClearMissingLogoPaths(sponsors);
        return sponsors;
    }

    public async Task<List<Sponsor>> GetActiveAsync()
    {
        var sponsors = await _context.Sponsors
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ClearMissingLogoPaths(sponsors);
        return sponsors;
    }

    public async Task AddAsync(Sponsor sponsor)
    {
        _context.Sponsors.Add(sponsor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sponsor sponsor)
    {
        var existingSponsor = await _context.Sponsors.FindAsync(sponsor.Id);

        if (existingSponsor is null)
            return;

        existingSponsor.Name = sponsor.Name;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.LogoPath = sponsor.LogoPath;
        existingSponsor.IsActive = sponsor.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sponsor = await _context.Sponsors.FindAsync(id);

        if (sponsor == null)
            return;

        _context.Sponsors.Remove(sponsor);

        await _context.SaveChangesAsync();
    }

    private void ClearMissingLogoPaths(IEnumerable<Sponsor> sponsors)
    {
        foreach (var sponsor in sponsors)
        {
            if (WebFilePathValidator.GetExistingPath(_environment, sponsor.LogoPath) is null)
            {
                sponsor.LogoPath = null;
            }
        }
    }
}
