using Application.Dtos;
using Application.Results;

namespace Application.Abstractions;

public interface INotifier
{
    string Name { get; }
    int Priority { get; }
    Task<Result> NotifySingle(UserListingPairDto userListingPair);
    
    /// <returns>Result with dictionary, if succeed - dictionary is empty, if not - key is user's id that was not notified and the error why </returns>
    Task<ResultWithClass<Dictionary<Guid, string>>> NotifyMultiple(UserListingPairDto[] userListingPairs);
}