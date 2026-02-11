using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.RemovePlayer;

/// <summary>
/// Command for removing a member from a team.
/// </summary>
/// <param name="Team">The team.</param>
/// <param name="MemberId">The member id to remove.</param>
public record RemoveMemberCommand(string Team, Guid MemberId)
    : IRequest<RemoveMemberCommand, Task<Result<Guid>>>, ITeamCommand;