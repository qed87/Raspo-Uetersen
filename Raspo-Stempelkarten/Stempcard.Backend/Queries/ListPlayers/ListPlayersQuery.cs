using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.ListPlayers;

[UsedImplicitly]
public class ListPlayersQuery(string streamId) : IStreamRequest<ListPlayersQuery, PlayerReadDto>
{
    public string StreamId { get; set; } = streamId;
}