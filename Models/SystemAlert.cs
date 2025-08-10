namespace WEBDOAN.Models;
public class SystemAlert
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string Severity { get; set; } // Info, Warning, Error
    public DateTime CreatedAt { get; set; }
}
