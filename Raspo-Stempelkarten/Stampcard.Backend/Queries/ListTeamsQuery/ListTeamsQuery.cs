using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListTeamsQuery;

/// <summary>
/// Gets a list of teams.
/// </summary>
[UsedImplicitly]
public record ListTeamsQuery : IRequest<ListTeamsQuery, Task<List<TeamReadDto>>>;