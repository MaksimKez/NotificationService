using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;

namespace Infrastructure.EmailNotifier;

public class EmailSender
{
    private readonly MailjetClient mailjetClient;
    private readonly IAsyncPolicy<Result> retryPolicy;

    public EmailSender(IOptions<MailjetSettings> mailjetOptions,
        IAsyncPolicy<Result> retryPolicy)
    {
        var mailjetSettings = mailjetOptions.Value;
        mailjetClient = new MailjetClient(mailjetSettings.ApiKey, mailjetSettings.SecretKey);
        this.retryPolicy = retryPolicy;
    }

    public Task<Result> SendEmailAsync(JObject email, CancellationToken cancellationToken = default)
        => retryPolicy.ExecuteAsync(async _ =>
        {
            var request = new MailjetRequest
            {
                Resource = SendV31.Resource
            }.Property(Send.Messages, email["Messages"]);

            var response = await mailjetClient.PostAsync(request);
            
            Console.WriteLine($"StatusCode: {response.StatusCode}\n");
            Console.WriteLine($"ErrorInfo: {response.GetErrorInfo()}\n");
            Console.WriteLine(response.GetData());
            Console.WriteLine($"ErrorMessage: {response.GetErrorMessage()}\n");

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(response.GetErrorMessage());
        }, cancellationToken);
}