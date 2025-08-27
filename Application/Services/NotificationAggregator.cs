using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Application.Services.Interfaces;

namespace Application.Services;

public class NotificationAggregator
    (IEnumerable<INotifier> notifiers,
    INotificationStrategy notificationStrategy)
    : INotificationAggregator
{   
    public Task<Result> NotifySingle(UserListingPairDto userListingPair)
    {
        return notificationStrategy.Notify(userListingPair, notifiers);
    }

    public async Task<Result> NotifyMultiple(UserListingPairDto[] userListingPairs)
    {
        foreach (var pair in userListingPairs)
        {
            var result = await NotifySingle(pair);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Success();
    }
}
