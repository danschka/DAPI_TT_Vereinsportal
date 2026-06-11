namespace TT_Website.Models;

public class ContentPageGalleryGroup
{
    public int Id { get; set; }
    public int ContentPageId { get; set; }
    public ContentPage? ContentPage { get; set; }
    public int GalleryGroupId { get; set; }
    public GalleryGroup? GalleryGroup { get; set; }
    public int SortOrder { get; set; }
}
