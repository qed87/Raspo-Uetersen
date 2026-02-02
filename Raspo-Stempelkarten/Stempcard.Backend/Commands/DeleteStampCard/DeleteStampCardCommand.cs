using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;

/// <summary>
/// Deletes a stamp card from a given team.
/// </summary>
public record DeleteStampCardCommand(Guid Id, string Team) 
    : IRequest<DeleteStampCardCommand, Task<Result<Unit>>>, ITeamCommand;