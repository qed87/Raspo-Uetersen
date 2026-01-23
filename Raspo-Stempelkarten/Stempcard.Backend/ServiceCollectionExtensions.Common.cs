using Castle.DynamicProxy;
using DispatchR.Abstractions.Notification;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddChangeTracking();
        services.AddTeamCommands();
        services.AddPlayerCommands();
        services.AddStampCardCommands();
        
        // Common
        services.AddScoped<IStampModelLoader, StampModelLoader>();
        services.AddTransient<IStampModelStorage, StampModelStorage>();
        services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
            KurrentDBClientSettings.Create(configuration.GetConnectionString("KurrentDb")!)));
        services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
            KurrentDBClientSettings.Create(configuration.GetConnectionString("KurrentDb")!)));
        return services;
    }
}