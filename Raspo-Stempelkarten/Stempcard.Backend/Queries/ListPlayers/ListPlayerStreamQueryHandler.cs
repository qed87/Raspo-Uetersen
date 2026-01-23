using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListPlayers;

public class ListPlayerStreamQueryHandler(IServiceProvider serviceProvider) : IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>
{
    public async IAsyncEnumerable<PlayerReadDto> Handle(ListPlayersQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.StreamId);
        foreach (var player in model.Players.Where(player => !player.Deleted))
        {
            var playerReadDto = new PlayerReadDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                Surname = player.Surname,
                BirthDate = player.Birthdate,
            };
            yield return playerReadDto;
        }
    }
}