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
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<NewsPost>> GetPublishedAsync()
    {
        return await _context.NewsPosts
            .AsNoTracking()
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
            .AsNoTracking()
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
        var existingNewsPost = await _context.NewsPosts.FindAsync(newsPost.Id);

        if (existingNewsPost is null)
            return;

        existingNewsPost.Title = newsPost.Title;
        existingNewsPost.Content = newsPost.Content;
        existingNewsPost.ImagePath = newsPost.ImagePath;
        existingNewsPost.CreatedAt = newsPost.CreatedAt;
        existingNewsPost.IsPublished = newsPost.IsPublished;

        await _context.SaveChangesAsync();
    }
}
