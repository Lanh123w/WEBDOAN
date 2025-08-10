namespace WEBDOAN.Models;

public class ContentItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; } // Post, Product, Service
    public string Tags { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
