using Application.Dtos;
using Application.Results;

namespace Application.Abstractions;

public interface INotifier
{
    string Name { get; }
    int Priority { get; }
    Task<Result> NotifySingle(UserListingPairDto userListingPair);
    Task<ResultWithClass<Dictionary<Guid, string>>> NotifyMultiple(UserListingPairDto[] userListingPairs);
}