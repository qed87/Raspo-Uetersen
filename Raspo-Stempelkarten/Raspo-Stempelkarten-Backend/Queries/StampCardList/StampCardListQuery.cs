using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardList;

public record StampCardListQuery(string Team, string Season) 
    : IRequest<StampCardListQuery, Task<Result<IEnumerable<StampCardReadDto>>>>;