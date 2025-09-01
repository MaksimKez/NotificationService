using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier;

public class EmailSenderFake : IEmailSender
{
    public async Task<Result> SendEmailAsync(JObject email, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return Result.Success();
    }
}