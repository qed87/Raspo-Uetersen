using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;

namespace StampCard.Backend.Queries.GetStampCardDetails;

/// <summary>
/// Gets detailed information about a stamp card.
/// </summary>
public record GetStampCardDetailsQuery(Guid Id, string Team) 
    : IRequest<GetStampCardDetailsQuery, Task<Result<StampCardReadDetailsDto?>>>, ITeamQuery;