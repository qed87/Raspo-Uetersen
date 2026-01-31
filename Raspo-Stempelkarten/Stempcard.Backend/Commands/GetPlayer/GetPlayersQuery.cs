using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetPlayer;

[UsedImplicitly]
public class GetPlayersQuery(string streamId) : IRequest<GetPlayersQuery, Task<PlayerReadDto?>>
{
    public string StreamId { get; set; } = streamId;
    
    public Guid Id { get; set; }
}