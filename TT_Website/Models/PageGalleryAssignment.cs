namespace TT_Website.Models;

public class PageGalleryAssignment
{
    public int Id { get; set; }
    public string PageKey { get; set; } = "";
    public string PageTitle { get; set; } = "";
    public bool SlideshowEnabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 3;

    public int GalleryGroupId { get; set; }
    public GalleryGroup? GalleryGroup { get; set; }
}
