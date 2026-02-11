using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.CreateStampCard;

/// <summary>
/// Creates a new stamp card for the given team and player.
/// </summary>
public record CreateStampCardCommand(string Team, Guid PlayerId, short AccountingYear, string Flag)
    : IRequest<CreateStampCardCommand, Task<Result<Unit>>>, ITeamCommand;
