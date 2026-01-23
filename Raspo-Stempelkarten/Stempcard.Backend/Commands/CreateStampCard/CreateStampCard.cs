using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.CreateStampCard;

public class CreateStampCard(string team) : IRequest<CreateStampCard, Task<Result<CreateStampCardResponse>>>
{
    public string Team { get; set; } = team;
    
    public short AccountingYear { get; set; }
    
    public Guid IssuedTo { get; set; }
    
}