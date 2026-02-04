using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.CreateStampCard;

/// <summary>
/// Creates a new stamp card for the given team and player.
/// </summary>
public record CreateStampCardCommand(string Team, Guid PlayerId, short AccountingYear)
    : IRequest<CreateStampCardCommand, Task<Result<Guid>>>, ITeamCommand;
