using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.AddMember;

/// <summary>
/// Command for adding a new member to the team.
/// </summary>
public record AddMemberCommand(string Team, string FirstName, string LastName, DateOnly Birthdate, string Birthplace)
    : IRequest<AddMemberCommand, Task<Result<Guid>>>, ITeamCommand;