using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class MemberApplicationService
{
    private readonly AppDbContext _context;

    public MemberApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MembershipApplication>> GetAllAsync()
    {
        return await _context.MemberApplications
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();
    }

    public async Task AddAsync(MembershipApplication application)
    {
        _context.MemberApplications.Add(application);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var application = await _context.MemberApplications.FindAsync(id);

        if (application == null)
            return;

        _context.MemberApplications.Remove(application);
        await _context.SaveChangesAsync();
    }
}