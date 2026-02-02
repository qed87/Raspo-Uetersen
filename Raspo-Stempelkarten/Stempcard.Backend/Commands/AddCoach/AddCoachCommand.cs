using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.AddCoach;

/// <summary>
/// Command for adding a coach to a team. A coach is able to modify an existing team.
/// </summary>
public record AddCoachCommand(string Team, string Email) : IRequest<AddCoachCommand, Task<Result<Unit>>>, ITeamCommand;