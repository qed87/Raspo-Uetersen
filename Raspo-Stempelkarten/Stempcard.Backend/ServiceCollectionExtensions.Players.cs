using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeletePlayer;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetPlayer;
using Raspo_Stempelkarten_Backend.Queries.ListPlayers;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerCommands(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<AddPlayersRequest, Task<Result<AddPlayersResponse>>>, AddPlayersRequestHandler>();
        services.AddScoped<IRequestHandler<DeletePlayerRequest, Task<Result<DeletePlayerResponse>>>, DeletePlayerRequestHandler>();
        services.AddScoped<IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>, ListPlayerStreamQueryHandler>();
        services.AddScoped<IRequestHandler<GetPlayersQuery, Task<PlayerReadDto?>>, GetPlayerQueryHandler>();

        return services;
    }
}