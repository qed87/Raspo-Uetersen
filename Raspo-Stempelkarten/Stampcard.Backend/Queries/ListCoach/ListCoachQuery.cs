using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Queries.ListCoach;

/// <summary>
/// Lists coaches of the team.
/// </summary>
[UsedImplicitly]
public record ListCoachQuery(string Team) : IRequest<ListCoachQuery, Task<Result<List<string>>>>, ITeamQuery;
