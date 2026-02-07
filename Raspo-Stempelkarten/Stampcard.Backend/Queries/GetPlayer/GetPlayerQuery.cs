using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetPlayer;

/// <summary>
/// Queries a team member by id.
/// </summary>
[UsedImplicitly]
public record GetPlayerQuery(string Team, Guid Id) : IRequest<GetPlayerQuery, Task<Result<PlayerReadDto?>>>, ITeamQuery;