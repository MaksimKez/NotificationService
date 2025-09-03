using Application.Dtos;
using Application.Results;

namespace Application.Services.Interfaces;

public interface INotificationAggregator
{
    Task<Result> NotifySingleAsync(UserListingPairDto userListingPair);
    Task<Result> NotifySingleAsync(EmailCodeDto emailCodeDto);
    Task<Result> NotifyMultipleAsync(UserListingPairDto[] userListingPairs);
}
