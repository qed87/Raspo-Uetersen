using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.RemoveCoach;

/// <summary>
/// Removes a coach from the team.
/// </summary>
public record RemoveCoachCommand(string Team, string Email)
    : IRequest<RemoveCoachCommand, Task<Result<Unit>>>, ITeamCommand;