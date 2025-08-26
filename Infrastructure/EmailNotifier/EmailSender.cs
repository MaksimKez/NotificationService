using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier;

public class EmailSender : IEmailSender
{
    public Task<Result> SendEmailAsync(JObject email)
    {
        throw new NotImplementedException();
    }
}