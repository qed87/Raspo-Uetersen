using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.StampStampCard;

/// <summary>
/// Stamps a stamp card.
/// </summary>
/// <param name="Team">The team the stamp card belongs to.</param>
/// <param name="StampCardId">The stamp card to stamp.</param>
/// <param name="Reason">The reason for the stamp (i.e. member activity).</param>
public record StampStampCardCommand(string Team, Guid StampCardId, string Reason) 
    : IRequest<StampStampCardCommand, Task<Result<Guid>>>, ITeamCommand;