using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.RemoveCoach;

/// <summary>
/// Removes a coach from the team.
/// </summary>
public record RemoveCoachCommand(string Team, string Email)
    : IRequest<RemoveCoachCommand, Task<Result<Unit>>>, ITeamCommand;