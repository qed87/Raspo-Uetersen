using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;

public record StampCardGetByIdQuery(string Season, string Team, Guid Id) 
    : IRequest<StampCardGetByIdQuery, Task<Result<StampCardReadDto>>>;