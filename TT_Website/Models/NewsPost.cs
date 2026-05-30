namespace TT_Website.Models;

public class NewsPost
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";

    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsPublished { get; set; } = true;
}