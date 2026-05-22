namespace TT_Website.Models
{
    public class DownloadDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
