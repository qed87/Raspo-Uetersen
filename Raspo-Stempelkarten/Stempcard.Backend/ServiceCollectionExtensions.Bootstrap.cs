using DispatchR.Abstractions.Notification;
using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using DispatchR.Extensions;
using FluentResults;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.AddCoach;
using Raspo_Stempelkarten_Backend.Commands.AddMember;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;
using Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.EraseStamp;
using Raspo_Stempelkarten_Backend.Commands.RemoveCoach;
using Raspo_Stempelkarten_Backend.Commands.RemoveMember;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StampStampCard;
using Raspo_Stempelkarten_Backend.Commands.UpdateTeam;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetMember;
using Raspo_Stempelkarten_Backend.Queries.GetStampCard;
using Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;
using Raspo_Stempelkarten_Backend.Queries.GetTeam;
using Raspo_Stempelkarten_Backend.Queries.ListCoach;
using Raspo_Stempelkarten_Backend.Queries.ListMembers;
using Raspo_Stempelkarten_Backend.Queries.ListStampCards;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend;

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
        services.AddScoped<INotificationHandler<MemberAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
        services.AddScoped<INotificationHandler<MemberRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
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
        services.AddScoped<IRequestHandler<AddMemberCommand, Task<Result<Guid>>>, AddMemberCommandHandler>();
        services.AddScoped<IRequestHandler<AddTeamCommand, Task<Result<Guid>>>, AddTeamCommandHandler>();
        services.AddScoped<IRequestHandler<CreateStampCardCommand, Task<Result<Guid>>>, CreateStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<CreateTeamStampCardsForAccountingYearsCommand, Task<Result<Unit>>>, CreateTeamStampCardsForAccountingYearsCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteStampCardCommand, Task<Result<Unit>>>, DeleteStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteTeamCommand, Task<Result<string>>>, DeleteTeamCommandHandler>();
        services.AddScoped<IRequestHandler<EraseStampCommand, Task<Result<Guid>>>, EraseStampCommandHandler>();
        services.AddScoped<IRequestHandler<RemoveCoachCommand, Task<Result<Unit>>>, RemoveCoachCommandHandler>();
        services.AddScoped<IRequestHandler<RemoveMemberCommand, Task<Result<Guid>>>, RemoveMemberCommandHandler>();
        services.AddScoped<IRequestHandler<StampStampCardCommand, Task<Result<Guid>>>, StampStampCardCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateTeamCommand, Task<Result<ulong>>>, UpdateTeamCommandHandler>();
        
        // queries
        services.AddScoped<IRequestHandler<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetCompletedStampCardsQueryHandler>();
        services.AddScoped<IRequestHandler<GetIncompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetIncompletedStampCardsQueryHandler>();
        services.AddScoped<IRequestHandler<GetMemberQuery, Task<MemberReadDto?>>, GetMemberQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>, GetStampCardQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>, GetStampCardDetailsQueryHandler>();
        services.AddScoped<IRequestHandler<GetTeamQuery, Task<TeamDetailedReadDto?>>, GetTeamQueryHandler>();
        services.AddScoped<IRequestHandler<ListCoachQuery, Task<List<string>>>, ListCoachQueryHandler>();
        services.AddScoped<IStreamRequestHandler<ListMembersQuery, MemberReadDto>, ListPlayerStreamQueryHandler>();
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