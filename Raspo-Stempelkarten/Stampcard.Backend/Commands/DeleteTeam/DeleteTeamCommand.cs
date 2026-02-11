using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.DeleteTeam;

/// <summary>
/// Delete team request.
/// </summary>
public record DeleteTeamCommand(string Team) : IRequest<DeleteTeamCommand, Task<Result<string>>>, ITeamCommand;