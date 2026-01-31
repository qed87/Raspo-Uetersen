using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListPlayers;

[UsedImplicitly]
public class ListPlayersQuery(string streamId) : IStreamRequest<ListPlayersQuery, PlayerReadDto>
{
    public string StreamId { get; set; } = streamId;
}