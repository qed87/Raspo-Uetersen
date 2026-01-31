using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetCompletedStampCardsQuery;

public record GetCompletedStampCardsQuery(string Team, int AccountingYear)
    : IRequest<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>
{
    public int NumberOfRequiredStamps { get; set; }
}