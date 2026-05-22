using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class GalleryService
{
    private readonly AppDbContext _context;

    public GalleryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GalleryImage>> GetAllAsync()
    {
        return await _context.GalleryImages
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(GalleryImage image)
    {
        _context.GalleryImages.Add(image);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var image = await _context.GalleryImages.FindAsync(id);

        if (image == null)
            return;

        _context.GalleryImages.Remove(image);
        await _context.SaveChangesAsync();
    }
}