using DispatchR.Abstractions.Notification;
using DispatchR.Extensions;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDispatchR(typeof(Program).Assembly);
        services.AddScoped<EventDataChangeTracker>();
        services.AddScoped<INotificationHandler<PlayerAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerDeleted>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<TeamAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<TeamDeleted>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampErased>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<CoachAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<CoachRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<IEventDataChangeTracker>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddTransient<ITeamService, TeamService>();
        services.AddScoped<ITeamModelLoader, TeamModelLoader>();
        services.AddTransient<IEventStorage, EventStorage>();
        
        // Common
        services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
            KurrentDBClientSettings.Create(configuration.GetConnectionString("KurrentDb")!)));
        services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
            KurrentDBClientSettings.Create(configuration.GetConnectionString("KurrentDb")!)));
        return services;
    }
}