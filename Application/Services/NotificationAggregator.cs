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
    private readonly IEnumerable<INotifier> _orderedNotifiers = notifiers.OrderBy(n => n.Priority).ToList();
    
    public Task<Result> NotifySingleAsync(UserListingPairDto userListingPair)
    {
        return notificationStrategy.Notify(userListingPair, _orderedNotifiers);
    }

    public Task<Result> NotifySingleAsync(EmailCodeDto emailCodeDto)
    {
        return notificationStrategy.Notify(emailCodeDto, notifiers);
    }

    public async Task<Result> NotifyMultipleAsync(UserListingPairDto[] userListingPairs)
    {
        foreach (var pair in userListingPairs)
        {
            var result = await NotifySingleAsync(pair);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Success();
    }
}
