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
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Sponsor>> GetActiveAsync()
    {
        return await _context.Sponsors
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
        _context.Sponsors.Update(sponsor);
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
