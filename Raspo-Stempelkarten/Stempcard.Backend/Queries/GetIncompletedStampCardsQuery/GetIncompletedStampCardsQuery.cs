using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;

public record GetIncompletedStampCardsQuery(string Team, int AccountingYear)
    : IRequest<GetIncompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>
{
    public int NumberOfRequiredStamps { get; set; }
}