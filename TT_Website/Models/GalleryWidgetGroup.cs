namespace TT_Website.Models;

public sealed record GalleryWidgetImage(int Id, string Title, string ImagePath);

public sealed record GalleryWidgetGroup(int Id, string Name, string? Description, List<GalleryWidgetImage> Images)
{
    public static List<GalleryWidgetGroup> FromGroups(IEnumerable<GalleryGroup> groups) => groups
        .Select(group => new GalleryWidgetGroup(
            group.Id,
            group.Name,
            group.Description,
            group.Images
                .OrderBy(x => x.SortOrder)
                .Where(x => x.GalleryImage is not null)
                .Select(x => new GalleryWidgetImage(x.GalleryImage!.Id, x.GalleryImage.Title, x.GalleryImage.ImagePath))
                .ToList()))
        .Where(group => group.Images.Count > 0)
        .ToList();
}
