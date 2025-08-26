using System.Net;
using Application.Results;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier;

public class EmailSender
    : IEmailSender
{
    private readonly RetryPolicySettings _retryPolicySettings;
    private readonly MailjetClient _mailjetClient;

    public EmailSender(IOptions<MailjetSettings> mailjetOptions,
        IOptions<RetryPolicySettings> retryPolicyOptions)
    {
        var mailjetSettings = mailjetOptions.Value;
        _retryPolicySettings = retryPolicyOptions.Value;
        _mailjetClient = new MailjetClient(mailjetSettings.ApiKey, mailjetSettings.SecretKey);
    }
    
    public async Task<Result> SendEmailAsync(JObject email, CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= _retryPolicySettings.MaxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                var request = new MailjetRequest
                {
                    Resource = SendV31.Resource
                }.Property(Send.Messages, email["Messages"]);

                var response = await _mailjetClient.PostAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return Result.Success();
                }

                var error = response.GetErrorMessage();

                if (attempt == _retryPolicySettings.MaxRetries ||
                                        response.StatusCode is < 500 and >= 400)
                {
                                //just to find error fast while debugging 
                    throw new ArgumentException(error);
                    return Result.Failure(error);
                }
                
                await Task.Delay(_retryPolicySettings.DelayMs * (int)Math.Pow(2, attempt - 1), cancellationToken);
            }
            catch (Exception) when (attempt < _retryPolicySettings.MaxRetries)  
            {
                if (attempt == _retryPolicySettings.MaxRetries - 1)
                    throw;

                await Task.Delay(_retryPolicySettings.DelayMs * (int)Math.Pow(2, attempt - 1), cancellationToken);
            }
        }
        
        return Result.Failure("Unexpected error occured");
    }
}