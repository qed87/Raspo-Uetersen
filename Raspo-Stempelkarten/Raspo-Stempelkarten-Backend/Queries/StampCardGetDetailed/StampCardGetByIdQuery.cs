using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;

public record StampCardGetByIdQuery(string Team, string Season, Guid Id) 
    : IRequest<StampCardGetByIdQuery, Task<Result<StampCardReadDto>>>;