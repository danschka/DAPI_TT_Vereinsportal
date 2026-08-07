using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public record GalleryGroupImageResult(bool Success, string Message);

public class GalleryService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public GalleryService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<List<GalleryImage>> GetAllAsync()
    {
        var images = await _context.GalleryImages
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

        return GetExistingImages(images);
    }

    public async Task<List<GalleryGroup>> GetGroupsAsync()
    {
        var groups = await _context.GalleryGroups
            .AsNoTracking()
            .Include(x => x.Images)
            .ThenInclude(x => x.GalleryImage)
            .OrderBy(x => x.Name)
            .ToListAsync();

        RemoveMissingGroupImages(groups);
        return groups;
    }

    public async Task<List<GalleryGroup>> GetPublicGroupsAsync(int count = 0)
    {
        var query = _context.GalleryGroups
            .AsNoTracking()
            .Where(x => x.IsPublic && x.Images.Any())
            .Include(x => x.Images.OrderBy(image => image.SortOrder))
            .ThenInclude(x => x.GalleryImage)
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable();
        var groups = await (count > 0 ? query.Take(count) : query).ToListAsync();
        RemoveMissingGroupImages(groups);
        return groups;
    }

    public async Task<List<GalleryGroup>> GetMemberGroupsAsync()
    {
        var groups = await _context.GalleryGroups.AsNoTracking()
            .Where(x => x.Images.Any())
            .Include(x => x.Images.OrderBy(image => image.SortOrder)).ThenInclude(x => x.GalleryImage)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        RemoveMissingGroupImages(groups);
        return groups;
    }

    public async Task<GalleryGroup?> GetGroupByIdAsync(int id)
    {
        var group = await _context.GalleryGroups
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(image => image.SortOrder))
            .ThenInclude(x => x.GalleryImage)
            .FirstOrDefaultAsync(x => x.Id == id);

        RemoveMissingGroupImages(group);
        return group;
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
        existingGroup.IsPublic = group.IsPublic;

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

    private List<GalleryImage> GetExistingImages(IEnumerable<GalleryImage> images)
    {
        return images
            .Where(image => WebFilePathValidator.Exists(_environment, image.ImagePath))
            .ToList();
    }

    private void RemoveMissingGroupImages(IEnumerable<GalleryGroup> groups)
    {
        foreach (var group in groups)
        {
            RemoveMissingGroupImages(group);
        }
    }

    private void RemoveMissingGroupImages(GalleryGroup? group)
    {
        if (group is null)
            return;

        group.Images = group.Images
            .Where(image => image.GalleryImage is not null
                && WebFilePathValidator.Exists(_environment, image.GalleryImage.ImagePath))
            .ToList();
    }
}
