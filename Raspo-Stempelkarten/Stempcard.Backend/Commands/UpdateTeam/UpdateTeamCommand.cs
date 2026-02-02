using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.UpdateTeam;

/// <summary>
/// Update a existing team name.
/// </summary>
public record UpdateTeamCommand(
    string Team,
    string Name,
    ulong ConcurrencyToken)
    : IRequest<UpdateTeamCommand, Task<Result<ulong>>>, ITeamCommand;
