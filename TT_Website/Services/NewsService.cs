using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class NewsService
{
    private readonly AppDbContext _context;

    public NewsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NewsPost>> GetAllAsync()
    {
        return await _context.NewsPosts
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<NewsPost>> GetPublishedAsync()
    {
        return await _context.NewsPosts
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(NewsPost newsPost)
    {
        _context.NewsPosts.Add(newsPost);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var newsPost = await _context.NewsPosts.FindAsync(id);

        if (newsPost == null)
            return;

        _context.NewsPosts.Remove(newsPost);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NewsPost>> GetLatestAsync(int count)
    {
        return await _context.NewsPosts
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<NewsPost?> GetByIdAsync(int id)
    {
        return await _context.NewsPosts.FindAsync(id);
    }

    public async Task UpdateAsync(NewsPost newsPost)
    {
        _context.NewsPosts.Update(newsPost);
        await _context.SaveChangesAsync();
    }
}