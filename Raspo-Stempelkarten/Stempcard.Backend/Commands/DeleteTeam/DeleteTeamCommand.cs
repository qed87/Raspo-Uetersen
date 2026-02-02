using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteTeam;

/// <summary>
/// Delete team request.
/// </summary>
public record DeleteTeamCommand(string Team) : IRequest<DeleteTeamCommand, Task<Result<string>>>, ITeamCommand;