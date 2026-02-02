using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCard;

/// <summary>
/// Gets a client representation of a stamp card entity.
/// </summary>
public record GetStampCardQuery(Guid Id, string Team) 
    : IRequest<GetStampCardQuery, Task<StampCardReadDto?>>, ITeamQuery;