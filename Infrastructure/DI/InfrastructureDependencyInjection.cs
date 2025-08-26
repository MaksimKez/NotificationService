using Application.Abstractions;
using Infrastructure.EmailNotifier;
using Infrastructure.EmailNotifier.EmailBuilder;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.DI;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<EmailSettings>(EmailSettings.DefaultConfigName);

        services.AddScoped<IEmailMessageBuilder, EmailBuilder>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<INotifier, EmailNotifier.EmailNotifier>();

        return services;
    }
}