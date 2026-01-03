using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

public class AddTeamRequest(string club, short birthCohort)
    : IRequest<AddTeamRequest, Task<Result<AddTeamResponse>>>
{
    public string Club { get; } = club;

    public short BirthCohort { get; } = birthCohort;
}