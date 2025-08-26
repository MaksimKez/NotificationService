namespace Infrastructure.EmailNotifier.Models;

public class RetryPolicySettings
{
    public const string DefaultConfigName = "RetryPolicySettings";
    public int MaxRetries { get; set; }
    public int DelayMs { get; set; }
}