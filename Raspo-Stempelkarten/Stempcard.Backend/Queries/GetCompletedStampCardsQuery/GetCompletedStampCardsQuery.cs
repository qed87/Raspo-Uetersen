using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;

/// <summary>
/// Returns all stamp cards that are completed.
/// </summary>
/// <param name="Team"></param>
/// <param name="AccountingYear"></param>
public record GetCompletedStampCardsQuery(string Team, int AccountingYear, int NumberOfRequiredStamps)
    : IRequest<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, ITeamQuery;