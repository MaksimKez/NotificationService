using Application.Abstractions;
using Application.Results;
using Infrastructure.EmailNotifier;
using Infrastructure.EmailNotifier.EmailBuilder;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Mailjet.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEmailMessageBuilder, EmailBuilder>();
        
        services.AddSingleton<IMailjetClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MailjetSettings>>().Value;
            return new MailjetClient(options.ApiKey, options.SecretKey);
        });
        
        services.AddSingleton<IAsyncPolicy<Result>>(sp =>
        {
            var retrySettings = sp.GetRequiredService<IOptions<RetryPolicySettings>>().Value;

            return Policy<Result>
                .HandleResult(r => !r.IsSuccess) 
                .Or<Exception>()
                .WaitAndRetryAsync(
                    retryCount: retrySettings.MaxRetries,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromMilliseconds(retrySettings.DelayMs * Math.Pow(2, attempt - 1))
                );
        });

        services.AddScoped<IEmailSender, EmailSender>();
        
        services.AddScoped<INotifier, EmailNotifier.EmailNotifier>();

        return services;
    }
}
