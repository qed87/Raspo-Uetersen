using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.GetPlayer;

/// <inheritdoc />
public class GetPlayerQueryHandler(IServiceProvider serviceProvider, ILogger<GetPlayerQueryHandler> logger) 
    : QueryHandlerBase<GetPlayerQuery, PlayerReadDto?>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<PlayerReadDto?> GetResult(ITeamAggregate model, GetPlayerQuery request)
    {
        var player = model.Players.SingleOrDefault(player => player.Id == request.Id && !player.Deleted);
        if (player is null) return Task.FromResult<PlayerReadDto?>(null);
        var playerReadDto = new PlayerReadDto(player.Id, player.FirstName, player.LastName, player.Birthdate, player.Birthplace);
        return Task.FromResult(playerReadDto)!;
    }
}