using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;

/// <summary>
/// Gets detailed information about a stamp card.
/// </summary>
public record GetStampCardDetailsQuery(Guid Id, string Team) 
    : IRequest<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>, ITeamQuery;