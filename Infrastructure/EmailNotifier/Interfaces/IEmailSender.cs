using Application.Results;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier.Interfaces;

public interface IEmailSender
{
    Task<Result> SendEmailAsync(JObject email);
}