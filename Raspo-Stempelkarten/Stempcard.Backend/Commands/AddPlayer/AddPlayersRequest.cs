using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddPlayer;

/// <inheritdoc />
public class AddPlayersRequest(string team)
    : IRequest<AddPlayersRequest, Task<Result<Guid>>>
{
    public string Team { get; } = team;
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateOnly Birthdate { get; set; }
    public string Birthplace { get; set; }
}