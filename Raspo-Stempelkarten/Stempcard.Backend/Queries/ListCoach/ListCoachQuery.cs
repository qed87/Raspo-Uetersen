using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Queries.ListCoach;

/// <summary>
/// Lists coaches of the team.
/// </summary>
[UsedImplicitly]
public record ListCoachQuery(string Team) : IRequest<ListCoachQuery, Task<List<string>>>, ITeamQuery;
