using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListPlayers;

/// <inheritdoc />
[UsedImplicitly]
public class ListPlayerStreamQueryHandler(IServiceProvider serviceProvider) : IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<PlayerReadDto> Handle(ListPlayersQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.StreamId);
        foreach (var player in model.Players.Where(player => !player.Deleted))
        {
            var playerReadDto = new PlayerReadDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                Surname = player.LastName,
                BirthDate = player.Birthdate,
            };
            yield return playerReadDto;
        }
    }
}