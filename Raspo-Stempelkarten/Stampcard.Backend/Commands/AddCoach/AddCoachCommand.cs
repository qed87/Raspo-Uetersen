using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.AddCoach;

/// <summary>
/// Command for adding a coach to a team. A coach is able to modify his teams.
/// </summary>
public record AddCoachCommand(string Team, string Email) : IRequest<AddCoachCommand, Task<Result<Unit>>>, ITeamCommand;