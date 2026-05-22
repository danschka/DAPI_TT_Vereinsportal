namespace TT_Website.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
