using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using StampCard.Backend.Services;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListPlayers;

/// <inheritdoc />
[UsedImplicitly]
public class ListPlayerStreamQueryHandler(IServiceProvider serviceProvider, 
    ILogger<ListPlayerStreamQueryHandler> logger) : IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<PlayerReadDto> Handle(ListPlayersQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        logger.LogTrace("Loading team from database...");
        var model = await modelLoader.LoadModelAsync(request.Team);
        if (model is null) yield break;
        foreach (var player in model.Players.Where(player => !player.Deleted))
        {
            var playerReadDto = new PlayerReadDto(player.Id, player.FirstName, player.LastName, 
                player.Birthdate, player.Birthplace);
            yield return playerReadDto;
        }
    }
}