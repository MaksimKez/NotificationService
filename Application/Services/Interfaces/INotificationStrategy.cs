using Application.Abstractions;
using Application.Dtos;
using Application.Results;

namespace Application.Services.Interfaces;

public interface INotificationStrategy
{
    Task<Result> Notify(UserListingPairDto userListingPair, IEnumerable<INotifier> notifiers);
    Task<Result> Notify(EmailCodeDto emailCode, IEnumerable<INotifier> notifiers);
}