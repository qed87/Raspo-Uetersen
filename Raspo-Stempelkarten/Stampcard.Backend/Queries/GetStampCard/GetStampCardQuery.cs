using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetStampCard;

/// <summary>
/// Gets a client representation of a stamp card entity.
/// </summary>
public record GetStampCardQuery(Guid Id, string Team) 
    : IRequest<GetStampCardQuery, Task<Result<StampCardReadDto?>>>, ITeamQuery;