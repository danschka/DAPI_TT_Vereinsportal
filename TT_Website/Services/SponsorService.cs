using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class SponsorService
{
    private readonly AppDbContext _context;

    public SponsorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sponsor>> GetAllAsync()
    {
        return await _context.Sponsors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Sponsor>> GetActiveAsync()
    {
        return await _context.Sponsors
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
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
}
