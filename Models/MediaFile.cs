namespace WEBDOAN.Models;
public class MediaFile
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; } // Image, Video, Document
    public string Url { get; set; }
    public DateTime UploadedAt { get; set; }
}
