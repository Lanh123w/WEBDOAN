namespace WEBDOAN.Models;
public class BackupRecord
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRestored { get; set; }
}
