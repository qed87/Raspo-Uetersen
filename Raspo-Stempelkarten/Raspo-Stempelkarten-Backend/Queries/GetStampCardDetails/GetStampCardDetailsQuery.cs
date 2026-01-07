using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCard;

public record GetStampCardDetailsQuery(Guid Id, string StreamId) : IRequest<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>
{
    public bool IncludeDetails { get; set; }
}