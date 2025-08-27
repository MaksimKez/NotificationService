namespace Infrastructure.EmailNotifier.Models;

public class MailjetSettings
{
    public const string DefaultConfigName = "MailjetKeys";
    public string SecretKey { get; set; }
    public string ApiKey { get; set; }
}