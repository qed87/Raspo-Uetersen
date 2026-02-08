using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetStampsQuery;

/// <summary>
/// Returns all stamps of a stamp card.
/// </summary>
[UsedImplicitly]
public record GetStampsQuery(string Team, Guid StampCardId)
    : IRequest<GetStampsQuery, Task<Result<List<StampReadDto>>>>, ITeamQuery;