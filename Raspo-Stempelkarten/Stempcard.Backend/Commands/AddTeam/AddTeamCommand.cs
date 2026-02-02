using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

/// <summary>
/// Add a new team.
/// </summary>
public record AddTeamCommand(string Club, string Name)
    : IRequest<AddTeamCommand, Task<Result<Guid>>>;