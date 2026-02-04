using DispatchR.Abstractions.Send;
using FluentResults;

namespace StampCard.Backend.Commands.AddTeam;

/// <summary>
/// Adds a new team.
/// </summary>
public record AddTeamCommand(string Club, string Name)
    : IRequest<AddTeamCommand, Task<Result<string>>>;