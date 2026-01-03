using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddPlayer;

public class AddPlayersRequest(string team)
    : IRequest<AddPlayersRequest, Task<Result<AddPlayersResponse>>>
{
    public string Team { get; } = team;
    
    public required string FirstName { get; set; }
    
    public required string Surname { get; set; }
    
    public DateOnly Birthdate { get; set; }
    
}