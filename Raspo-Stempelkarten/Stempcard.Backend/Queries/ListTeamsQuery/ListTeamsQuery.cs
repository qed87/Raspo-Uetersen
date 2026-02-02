using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

/// <summary>
/// Gets a list of teams.
/// </summary>
[UsedImplicitly]
public record ListTeamsQuery : IRequest<ListTeamsQuery, Task<List<TeamReadDto>>>;