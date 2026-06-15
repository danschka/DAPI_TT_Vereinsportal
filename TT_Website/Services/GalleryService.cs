using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public record GalleryGroupImageResult(bool Success, string Message);

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
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<GalleryGroup>> GetGroupsAsync()
    {
        return await _context.GalleryGroups
            .AsNoTracking()
            .Include(x => x.Images)
            .ThenInclude(x => x.GalleryImage)
            .Include(x => x.PageAssignments)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<GalleryGroup?> GetGroupByIdAsync(int id)
    {
        return await _context.GalleryGroups
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(image => image.SortOrder))
            .ThenInclude(x => x.GalleryImage)
            .Include(x => x.PageAssignments)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PageGalleryAssignment?> GetPageAssignmentAsync(string pageKey)
    {
        return await _context.PageGalleryAssignments
            .AsNoTracking()
            .Include(x => x.GalleryGroup)
            .ThenInclude(x => x!.Images.OrderBy(image => image.SortOrder))
            .ThenInclude(x => x.GalleryImage)
            .FirstOrDefaultAsync(x => x.PageKey == pageKey);
    }

    public async Task<GalleryImage> AddAsync(GalleryImage image)
    {
        _context.GalleryImages.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<GalleryGroup> AddGroupAsync(GalleryGroup group)
    {
        _context.GalleryGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task UpdateGroupAsync(GalleryGroup group)
    {
        var existingGroup = await _context.GalleryGroups.FindAsync(group.Id);

        if (existingGroup is null)
            return;

        existingGroup.Name = group.Name;
        existingGroup.Description = group.Description;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.GalleryGroups.FindAsync(id);

        if (group == null)
            return;

        _context.GalleryGroups.Remove(group);
        await _context.SaveChangesAsync();
    }

    public async Task<GalleryGroupImageResult> AddImageToGroupAsync(int groupId, int imageId)
    {
        var groupExists = await _context.GalleryGroups.AnyAsync(x => x.Id == groupId);
        if (!groupExists)
            return new GalleryGroupImageResult(false, "Die Bildgruppe wurde nicht gefunden.");

        var imageExists = await _context.GalleryImages.AnyAsync(x => x.Id == imageId);
        if (!imageExists)
            return new GalleryGroupImageResult(false, "Das ausgewählte Bild wurde nicht gefunden.");

        var exists = await _context.GalleryGroupImages
            .AnyAsync(x => x.GalleryGroupId == groupId && x.GalleryImageId == imageId);

        if (exists)
            return new GalleryGroupImageResult(false, "Dieses Bild ist bereits in der Gruppe.");

        var nextSortOrder = await _context.GalleryGroupImages
            .Where(x => x.GalleryGroupId == groupId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? 0;

        _context.GalleryGroupImages.Add(new GalleryGroupImage
        {
            GalleryGroupId = groupId,
            GalleryImageId = imageId,
            SortOrder = nextSortOrder + 1
        });

        await _context.SaveChangesAsync();
        return new GalleryGroupImageResult(true, "Bild wurde der Gruppe hinzugefügt.");
    }

    public async Task RemoveImageFromGroupAsync(int groupImageId)
    {
        var groupImage = await _context.GalleryGroupImages.FindAsync(groupImageId);

        if (groupImage == null)
            return;

        _context.GalleryGroupImages.Remove(groupImage);
        await _context.SaveChangesAsync();
    }

    public async Task SavePageAssignmentAsync(PageGalleryAssignment assignment)
    {
        var existing = await _context.PageGalleryAssignments
            .FirstOrDefaultAsync(x => x.PageKey == assignment.PageKey);

        if (existing == null)
        {
            _context.PageGalleryAssignments.Add(assignment);
        }
        else
        {
            existing.PageTitle = assignment.PageTitle;
            existing.GalleryGroupId = assignment.GalleryGroupId;
            existing.SlideshowEnabled = assignment.SlideshowEnabled;
            existing.IntervalSeconds = assignment.IntervalSeconds;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeletePageAssignmentAsync(int id)
    {
        var assignment = await _context.PageGalleryAssignments.FindAsync(id);

        if (assignment == null)
            return;

        _context.PageGalleryAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var image = await _context.GalleryImages.FindAsync(id);

        if (image == null)
            return;

        var groupImages = await _context.GalleryGroupImages
            .Where(x => x.GalleryImageId == id)
            .ToListAsync();

        _context.GalleryGroupImages.RemoveRange(groupImages);
        _context.GalleryImages.Remove(image);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GalleryImage>> GetLatestAsync(int count)
    {
        return await _context.GalleryImages
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAt)
            .Take(count)
            .ToListAsync();
    }
}
