namespace Infrastructure.EmailNotifier.Models;

public class EmailSettings
{
    public const string DefaultConfigName = "EmailSettings";
    public string FromName { get; set; }
    public string FromEmail { get; set; }
    public string ToEmail { get; set; }
}