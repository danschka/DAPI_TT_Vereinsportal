using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class ContentPageService
{
    private readonly AppDbContext _context;

    public ContentPageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContentPage>> GetAllAsync()
    {
        return await _context.ContentPages
            .AsNoTracking()
            .Include(x => x.Parent)
            .Include(x => x.GalleryGroups)
            .ThenInclude(x => x.GalleryGroup)
            .OrderBy(x => x.ParentId != null)
            .ThenBy(x => x.ParentId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<List<ContentPage>> GetNavigationAsync()
    {
        return await _context.ContentPages
            .Include(x => x.Children.Where(child => child.IsActive && child.ShowInNavigation).OrderBy(child => child.SortOrder))
            .Where(x => x.IsActive && x.ShowInNavigation && x.ParentId == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ContentPage?> GetByIdAsync(int id)
    {
        return await _context.ContentPages
            .AsNoTracking()
            .Include(x => x.GalleryGroups)
            .ThenInclude(x => x.GalleryGroup)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ContentPage?> GetBySlugAsync(string slug)
    {
        return await _context.ContentPages
            .Include(x => x.GalleryGroups.OrderBy(group => group.SortOrder))
            .ThenInclude(x => x.GalleryGroup)
            .ThenInclude(x => x!.Images.OrderBy(image => image.SortOrder))
            .ThenInclude(x => x.GalleryImage)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
    }

    public async Task SaveAsync(ContentPage page)
    {
        page.Slug = NormalizeSlug(page.Slug);
        page.UpdatedAt = DateTime.Now;

        if (page.Id == 0)
        {
            page.Title = page.Title.Trim();
            page.Summary = page.Summary?.Trim();
            page.Content = page.Content.Trim();
            page.ExternalUrl = page.ExternalUrl?.Trim();
            _context.ContentPages.Add(page);
        }
        else
        {
            var existing = await _context.ContentPages.FindAsync(page.Id);

            if (existing is null)
                return;

            existing.Title = page.Title.Trim();
            existing.Slug = page.Slug;
            existing.Summary = page.Summary?.Trim();
            existing.Content = page.Content.Trim();
            existing.ExternalUrl = page.ExternalUrl?.Trim();
            existing.SortOrder = page.SortOrder;
            existing.IsActive = page.IsActive;
            existing.ShowInNavigation = page.ShowInNavigation;
            existing.ShowMap = page.ShowMap;
            existing.MapAddress = page.MapAddress?.Trim();
            existing.MapEmbedUrl = page.MapEmbedUrl?.Trim();
            existing.ParentId = page.ParentId;
            existing.UpdatedAt = page.UpdatedAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateContentAsync(ContentPage page)
    {
        var existing = await _context.ContentPages.FindAsync(page.Id);

        if (existing is null)
            return;

        existing.Title = page.Title.Trim();
        existing.Summary = page.Summary?.Trim();
        existing.Content = page.Content?.Trim() ?? "";
        existing.ExternalUrl = page.ExternalUrl?.Trim();

        if (existing.Slug == "vereinslokal")
        {
            existing.ShowMap = page.ShowMap;
            existing.MapAddress = page.MapAddress?.Trim();
            existing.MapEmbedUrl = page.MapEmbedUrl?.Trim();
        }

        existing.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var page = await _context.ContentPages
            .Include(x => x.Children)
            .Include(x => x.GalleryGroups)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (page is null || page.Children.Count > 0)
            return;

        _context.ContentPageGalleryGroups.RemoveRange(page.GalleryGroups);
        _context.ContentPages.Remove(page);
        await _context.SaveChangesAsync();
    }

    public async Task AssignGalleryGroupAsync(int pageId, int groupId)
    {
        var exists = await _context.ContentPageGalleryGroups
            .AnyAsync(x => x.ContentPageId == pageId && x.GalleryGroupId == groupId);

        if (exists)
            return;

        var nextOrder = await _context.ContentPageGalleryGroups
            .Where(x => x.ContentPageId == pageId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? 0;

        _context.ContentPageGalleryGroups.Add(new ContentPageGalleryGroup
        {
            ContentPageId = pageId,
            GalleryGroupId = groupId,
            SortOrder = nextOrder + 1
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveGalleryAssignmentAsync(int assignmentId)
    {
        var assignment = await _context.ContentPageGalleryGroups.FindAsync(assignmentId);

        if (assignment is null)
            return;

        _context.ContentPageGalleryGroups.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public static string NormalizeSlug(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("ä", "ae")
            .Replace("ö", "oe")
            .Replace("ü", "ue")
            .Replace("ß", "ss")
            .Replace(" ", "-");
    }
}
