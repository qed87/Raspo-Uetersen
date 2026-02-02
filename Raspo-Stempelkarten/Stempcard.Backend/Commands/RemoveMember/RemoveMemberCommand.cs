using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.RemoveMember;

/// <summary>
/// Command for removing a member from a team.
/// </summary>
/// <param name="Team">The team.</param>
/// <param name="MemberId">The member id to remove.</param>
public record RemoveMemberCommand(string Team, Guid MemberId)
    : IRequest<RemoveMemberCommand, Task<Result<Guid>>>, ITeamCommand;