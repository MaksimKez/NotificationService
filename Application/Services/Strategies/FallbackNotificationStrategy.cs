using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Application.Services.Interfaces;

namespace Application.Services.Strategies;

public class FallbackNotificationStrategy : INotificationStrategy
{
    public async Task<Result> Notify(UserListingPairDto userListingPair, IEnumerable<INotifier> notifiers)
    {
        foreach (var notifier in notifiers)
        {
            var result = await notifier.NotifySingle(userListingPair);
            if (result.IsSuccess)
                return result;
        }
        return Result.Failure("All notification channel failed.");
    }

    public async Task<Result> Notify(EmailCodeDto emailCode, IEnumerable<INotifier> notifiers)
    {
        foreach (var notifier in notifiers)
        {
            var result = await notifier.NotifySingle(emailCode);
            if (result.IsSuccess)
                return result;
        }
        return Result.Failure("All notification channel failed.");
    }
}
