using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetTeam;

/// <summary>
/// Gets the team
/// </summary>
[UsedImplicitly]
public record GetTeamQuery(string Team) : IRequest<GetTeamQuery, Task<TeamDetailedReadDto?>>, ITeamQuery;
