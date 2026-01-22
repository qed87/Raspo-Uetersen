using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddTeamCommands(this IServiceCollection services)
    {
        // Commands Teams
        services.AddScoped<IRequestHandler<AddTeamRequest, Task<Result<AddTeamResponse>>>, AddTeamRequestHandler>();
        services.AddScoped<IRequestHandler<DeleteTeamRequest, Task<Result>>, DeleteTeamRequestHandler>();
        services.AddScoped<IRequestHandler<ListTeamsQuery, Task<List<string>>>, ListTeamsQueryHandler>();

        return services;
    }
}