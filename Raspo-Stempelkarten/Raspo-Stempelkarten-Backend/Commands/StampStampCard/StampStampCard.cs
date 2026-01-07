using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampStampCard;

public class StampStampCard(string team, Guid stampCardId) : IRequest<StampStampCard, Task<Result<StampStampCardResponse>>>
{
    public string Team { get; set; } = team;

    public Guid StampCardId { get; set; } = stampCardId;
    
    public required string Reason { get; set; }
}