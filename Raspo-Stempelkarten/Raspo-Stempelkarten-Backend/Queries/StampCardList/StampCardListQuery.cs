using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardList;

public record StampCardListQuery(string Season, string Team) 
    : IRequest<StampCardListQuery, Task<Result<IEnumerable<StampCardReadDto>>>>;