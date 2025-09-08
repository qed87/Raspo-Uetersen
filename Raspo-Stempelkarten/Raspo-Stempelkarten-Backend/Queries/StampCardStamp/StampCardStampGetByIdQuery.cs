using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

public record StampCardStampGetByIdQuery(string Team, string Season, Guid StampCardId, Guid Id) 
    : IRequest<StampCardStampGetByIdQuery, Task<Result<StampReadDto>>>;