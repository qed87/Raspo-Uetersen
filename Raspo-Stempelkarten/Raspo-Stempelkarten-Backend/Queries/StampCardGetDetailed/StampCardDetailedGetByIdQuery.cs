using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;

public record StampCardDetailedGetByIdQuery(string Team, string Season, Guid Id) 
    : IRequest<StampCardDetailedGetByIdQuery, Task<Result<StampCardReadDetailsDto>>>;