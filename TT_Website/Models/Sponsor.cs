namespace TT_Website.Models
{
    public class Sponsor
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? WebsiteUrl { get; set; }
        public string? LogoPath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
