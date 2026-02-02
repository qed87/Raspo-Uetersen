using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.CreateStampCard;

/// <summary>
/// Creates a new stamp card for the given team.
/// </summary>
public record CreateStampCardCommand(string Team, Guid MemberId, short AccountingYear)
    : IRequest<CreateStampCardCommand, Task<Result<Guid>>>, ITeamCommand;
