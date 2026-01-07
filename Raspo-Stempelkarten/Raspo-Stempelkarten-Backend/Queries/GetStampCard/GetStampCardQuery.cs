using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCard;

public record GetStampCardQuery(Guid Id, string StreamId) : IRequest<GetStampCardQuery, Task<StampCardReadDto?>>;