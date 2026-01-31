using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

/// <summary>
/// Add a new team.
/// </summary>
/// <param name="club">The club name.</param>
/// <param name="name">The team name.</param>
/// <param name="issuedBy">The issuer.</param>
public class AddTeamRequest(string club, string name, string issuedBy)
    : IRequest<AddTeamRequest, Task<Result<Guid>>>
{
    public string Club { get; } = club;
    public string Name { get; } = name;
    public string IssuedBy { get; } = issuedBy;
    public DateTimeOffset IssuedDate { get; } = DateTime.UtcNow;
}