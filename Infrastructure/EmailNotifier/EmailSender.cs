using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using Polly;

namespace Infrastructure.EmailNotifier;

public class EmailSender(IMailjetClient mailjetClient, IAsyncPolicy<Result> retryPolicy)
    : IEmailSender
{
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