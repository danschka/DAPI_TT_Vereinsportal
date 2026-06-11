namespace TT_Website.Models;

public class GalleryGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<GalleryGroupImage> Images { get; set; } = new();
    public List<PageGalleryAssignment> PageAssignments { get; set; } = new();
    public List<ContentPageGalleryGroup> ContentPageAssignments { get; set; } = new();
}
