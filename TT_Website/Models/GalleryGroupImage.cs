namespace TT_Website.Models;

public class GalleryGroupImage
{
    public int Id { get; set; }
    public int GalleryGroupId { get; set; }
    public GalleryGroup? GalleryGroup { get; set; }

    public int GalleryImageId { get; set; }
    public GalleryImage? GalleryImage { get; set; }

    public int SortOrder { get; set; }
}
