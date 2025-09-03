using Application.Dtos.Settings.Contracts;

namespace Application.Dtos.Settings;

public class EmailNotifierSettings : INotifierSettings
{
    public const string DefaultConfigName = "EmailNotifierSettings";
    public string Name { get; init; }
    public int Priority { get; init; }
}