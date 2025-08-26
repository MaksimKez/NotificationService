using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;

namespace Infrastructure.EmailNotifier;

public class EmailSender : IEmailSender
{
    private readonly MailjetClient _mailjetClient;
    private readonly IAsyncPolicy<Result> _retryPolicy;

    public EmailSender(IOptions<MailjetSettings> mailjetOptions,
        IAsyncPolicy<Result> retryPolicy)
    {
        var mailjetSettings = mailjetOptions.Value;
        _mailjetClient = new MailjetClient(mailjetSettings.ApiKey, mailjetSettings.SecretKey);
        _retryPolicy = retryPolicy;
    }

    public Task<Result> SendEmailAsync(JObject email, CancellationToken cancellationToken = default)
        => _retryPolicy.ExecuteAsync(async _ =>
        {
            var request = new MailjetRequest
            {
                Resource = SendV31.Resource
            }.Property(Send.Messages, email["Messages"]);

            var response = await _mailjetClient.PostAsync(request);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(response.GetErrorMessage());
        }, cancellationToken);
}