using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Models;

namespace Infrastructure.EmailNotifier;

public class EmailNotifier(IEmailMessageBuilder messageBuilder) : INotifier
{
    public string Name { get; } = "EmailNotifier";
    public int Priority { get; } = 2;
    public Task<Result> NotifySingle(UserListingPairDto userListingPair)
    {
        var emailMessage = messageBuilder.BuildDefault(userListingPair.Listing, userListingPair.User.Email
                                                                                ?? throw new ArgumentException());
        //todo send email

        throw new NotImplementedException();
    }

    public Task<Result> NotifyMultiple(UserListingPairDto[] userListingPairs)
    {
        foreach (var userListingPair in userListingPairs)
        {
            var emailMessage = messageBuilder.BuildDefault(userListingPair.Listing, userListingPair.User.Email
                ?? throw new ArgumentException());

            //todo send email
        }
        throw new NotImplementedException();
    }
}