using DispatchR.Abstractions.Notification;
using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using DispatchR.Extensions;
using FluentResults;
using KurrentDB.Client;
using StampCard.Backend.Commands.AddCoach;
using StampCard.Backend.Commands.AddPlayer;
using StampCard.Backend.Commands.AddTeam;
using StampCard.Backend.Commands.CreateStampCard;
using StampCard.Backend.Commands.DeleteStampCard;
using StampCard.Backend.Commands.DeleteTeam;
using StampCard.Backend.Commands.EraseStamp;
using StampCard.Backend.Commands.RemoveCoach;
using StampCard.Backend.Commands.RemovePlayer;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using StampCard.Backend.Commands.StampStampCard;
using StampCard.Backend.Commands.UpdatePlayer;
using StampCard.Backend.Commands.UpdateTeam;
using StampCard.Backend.Events;
using StampCard.Backend.Queries.GetCompletedStampCardsQuery;
using StampCard.Backend.Queries.GetIncompletedStampCardsQuery;
using StampCard.Backend.Queries.GetPlayer;
using StampCard.Backend.Queries.GetStampCard;
using StampCard.Backend.Queries.GetStampCardDetails;
using StampCard.Backend.Queries.GetStampsQuery;
using StampCard.Backend.Queries.GetTeam;
using StampCard.Backend.Queries.ListCoach;
using StampCard.Backend.Queries.ListPlayers;
using StampCard.Backend.Queries.ListStampCards;
using StampCard.Backend.Queries.ListTeamsQuery;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend;

/// <summary>
/// The service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDispatchR(options => {});
        services.AddHttpContextAccessor();
        services.AddTransient<IUserProvider, DefaultUserProvider>();
        services.AddScoped<EventDataChangeTracker>();
        
        // notifications
        services.AddScoped<INotificationHandler<TeamUpdated>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerUpdated>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<PlayerRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<TeamAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<TeamDeleted>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampCardRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<StampErased>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<CoachAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<CoachRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<IEventDataChangeTracker>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        
        // commands
        services.AddScoped<IRequestHandler<AddCoachCommand, Task<Result<Unit>>>, AddCoachCommandHandler>();
        services.AddScoped<IRequestHandler<AddPlayerCommand, Task<Result<Guid>>>, AddPlayerCommandHandler>();
        services.AddScoped<IRequestHandler<UpdatePlayerCommand, Task<Result<Unit>>>, UpdatePlayerCommandHandler>();
        services.AddScoped<IRequestHandler<AddTeamCommand, Task<Result<string>>>, AddTeamCommandHandler>();
        services.AddScoped<IRequestHandler<CreateStampCardCommand, Task<Result<Unit>>>, CreateStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteStampCardCommand, Task<Result<Unit>>>, DeleteStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteTeamCommand, Task<Result<string>>>, DeleteTeamCommandHandler>();
        services.AddScoped<IRequestHandler<EraseStampCommand, Task<Result<Guid>>>, EraseStampCommandHandler>();
        services.AddScoped<IRequestHandler<RemoveCoachCommand, Task<Result<Unit>>>, RemoveCoachCommandHandler>();
        services.AddScoped<IRequestHandler<RemoveMemberCommand, Task<Result<Guid>>>, RemoveMemberCommandHandler>();
        services.AddScoped<IRequestHandler<StampStampCardCommand, Task<Result<Guid>>>, StampStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateTeamCommand, Task<Result<ulong>>>, UpdateTeamCommandHandler>();
        
        // queries
        services.AddScoped<IRequestHandler<GetCompletedStampCardsQuery, Task<Result<List<StampCardReadDetailsDto>>>>, 
            GetCompletedStampCardsQueryHandler>();
        services.AddScoped<IRequestHandler<GetIncompletedStampCardsQuery, Task<Result<List<StampCardReadDetailsDto>>>>, 
            GetIncompletedStampCardsQueryHandler>();
        services.AddScoped<IRequestHandler<GetPlayerQuery, Task<Result<PlayerReadDto?>>>, GetPlayerQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampCardQuery, Task<Result<StampCardReadDto?>>>, GetStampCardQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampCardDetailsQuery, Task<Result<StampCardReadDetailsDto?>>>, GetStampCardDetailsQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampsQuery, Task<Result<List<StampReadDto>>>>, GetStampQueryHandler>();
        services.AddScoped<IRequestHandler<GetTeamQuery, Task<Result<TeamDetailedReadDto?>>>, GetTeamQueryHandler>();
        services.AddScoped<IRequestHandler<ListCoachQuery, Task<Result<List<string>>>>, ListCoachQueryHandler>();
        services.AddScoped<IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>, ListPlayerStreamQueryHandler>();
        services.AddScoped<IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>, ListStampCardQueryHandler>();
        services.AddScoped<IRequestHandler<ListTeamsQuery, Task<List<TeamReadDto>>>, ListTeamsQueryHandler>();
        
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