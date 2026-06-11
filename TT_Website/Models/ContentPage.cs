namespace TT_Website.Models;

public class ContentPage
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public string Content { get; set; } = "";
    public string? ExternalUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInNavigation { get; set; } = true;
    public bool ShowMap { get; set; }
    public string? MapAddress { get; set; }
    public string? MapEmbedUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int? ParentId { get; set; }
    public ContentPage? Parent { get; set; }
    public List<ContentPage> Children { get; set; } = new();
    public List<ContentPageGalleryGroup> GalleryGroups { get; set; } = new();
}
