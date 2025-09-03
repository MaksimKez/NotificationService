namespace Application.Dtos.Settings.Contracts;

public interface INotifierSettings
{
    public string Name { get; init; }
    public int Priority { get; init; }
}