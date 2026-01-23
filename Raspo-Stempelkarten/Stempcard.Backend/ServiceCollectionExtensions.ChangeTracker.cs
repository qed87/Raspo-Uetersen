using Castle.DynamicProxy;
using DispatchR.Abstractions.Notification;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddChangeTracking(this IServiceCollection services)
    {
        services.AddScoped<EventDataChangeTracker>();

        // Change Tracker 
        services.AddScoped<IEventDataChangeTracker, EventDataChangeTracker>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerAdded>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerDeleted>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<TeamAdded>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardAdded>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardRemoved>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampAdded>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampErased>>(provider =>
            provider.GetRequiredService<EventDataChangeTracker>());
        services.AddSingleton<IProxyGenerator, ProxyGenerator>();

        return services;
    }
}