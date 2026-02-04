using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.DeleteStampCard;

/// <summary>
/// Deletes a stamp card from a given team.
/// </summary>
public record DeleteStampCardCommand(Guid Id, string Team) 
    : IRequest<DeleteStampCardCommand, Task<Result<Unit>>>, ITeamCommand;