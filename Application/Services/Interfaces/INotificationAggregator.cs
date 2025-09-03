using Application.Dtos;
using Application.Results;

namespace Application.Services.Interfaces;

public interface INotificationAggregator
{
    Task<Result> NotifySingle(UserListingPairDto userListingPair);
    Task<Result> NotifySingle(EmailCodeDto emailCodeDto);
    Task<Result> NotifyMultiple(UserListingPairDto[] userListingPairs);
}
