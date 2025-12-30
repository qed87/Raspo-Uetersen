using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

public record StampCardStampGetByIdQuery(string Season, string Team, Guid StampCardId, Guid Id) 
    : IRequest<StampCardStampGetByIdQuery, Task<Result<StampReadDto>>>;