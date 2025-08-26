using Application.Services;
using Application.Services.Interfaces;
using Application.Services.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI;


public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationStrategy, FallbackNotificationStrategy>();
        services.AddScoped<INotificationAggregator, NotificationAggregator>();
        
        return services;
    }
}
