using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class DownloadService
{
    private readonly AppDbContext _context;

    public DownloadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DownloadDocument>> GetAllAsync()
    {
        return await _context.DownloadDocuments
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(DownloadDocument document)
    {
        _context.DownloadDocuments.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task<DownloadDocument?> GetByIdAsync(int id)
    {
        return await _context.DownloadDocuments.FindAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var document = await _context.DownloadDocuments.FindAsync(id);

        if (document == null)
            return;

        _context.DownloadDocuments.Remove(document);
        await _context.SaveChangesAsync();
    }
}
