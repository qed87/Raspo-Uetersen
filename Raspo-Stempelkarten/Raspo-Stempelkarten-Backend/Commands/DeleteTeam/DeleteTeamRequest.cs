using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteTeam;

public record DeleteTeamRequest(string Id) : IRequest<DeleteTeamRequest, Task<Result>>;