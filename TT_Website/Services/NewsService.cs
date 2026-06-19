using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class NewsService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public NewsService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<List<NewsPost>> GetAllAsync()
    {
        var posts = await _context.NewsPosts
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        ClearMissingImagePaths(posts);
        return posts;
    }

    public async Task<List<NewsPost>> GetPublishedAsync()
    {
        var posts = await _context.NewsPosts
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        ClearMissingImagePaths(posts);
        return posts;
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
        var posts = await _context.NewsPosts
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        ClearMissingImagePaths(posts);
        return posts;
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

    private void ClearMissingImagePaths(IEnumerable<NewsPost> posts)
    {
        foreach (var post in posts)
        {
            if (WebFilePathValidator.GetExistingPath(_environment, post.ImagePath) is null)
            {
                post.ImagePath = null;
            }
        }
    }
}
