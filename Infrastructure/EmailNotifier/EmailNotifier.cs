using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Interfaces;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier;

public class EmailNotifier
    (IEmailMessageBuilder messageBuilder, 
     IEmailSender sender) 
    : INotifier
{
    public string Name { get; } = "EmailNotifier";
    public int Priority { get; } = 2;
    public async Task<Result> NotifySingle(UserListingPairDto userListingPair)
    {
        var emailMessage = messageBuilder.BuildDefault(userListingPair.Listing, userListingPair.User.Email
                                                                                ?? throw new ArgumentException());
        return await SendMessage(emailMessage);
    }

    public async Task<Result> NotifySingle(EmailCodeDto emailCodeDto)
    {
        var message = $"Code: {emailCodeDto.Token}";
        var emailMessage = messageBuilder.WithMessage(message).WithSubject("Email verification").FromTo(null, null, emailCodeDto.ToEmail).Build();

        return await SendMessage(emailMessage);
    }

    public async Task<ResultWithClass<Dictionary<Guid, string>>> NotifyMultiple(UserListingPairDto[] userListingPairs)
    {
        var unnotifiedUsersWithErrors = new Dictionary<Guid, string>();
        
        foreach (var userListingPair in userListingPairs)
        {
            var emailMessage = messageBuilder.BuildDefault(userListingPair.Listing, userListingPair.User.Email
                ?? throw new ArgumentException());

            var result = await sender.SendEmailAsync(emailMessage);
            if (!result.IsSuccess)
            {
                unnotifiedUsersWithErrors.Add(userListingPair.User.Id, result.Error);
            }
        }

        return unnotifiedUsersWithErrors.Count != 0 
            ? ResultWithClass<Dictionary<Guid, string>>.PartialFailure(unnotifiedUsersWithErrors)
            : ResultWithClass<Dictionary<Guid, string>>.Success(new Dictionary<Guid, string>());
    }

    private async Task<Result> SendMessage(JObject emailMessage)
    {
        var result = await sender.SendEmailAsync(emailMessage);
        return !result.IsSuccess
            ? result 
            : Result.Success();
    }
}