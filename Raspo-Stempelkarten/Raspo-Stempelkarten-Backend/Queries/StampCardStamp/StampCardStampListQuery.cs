using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

public record StampCardStampListQuery(string Team, string Season, Guid Id) 
    : IRequest<StampCardStampListQuery, Task<Result<IEnumerable<StampReadDto>>>>;