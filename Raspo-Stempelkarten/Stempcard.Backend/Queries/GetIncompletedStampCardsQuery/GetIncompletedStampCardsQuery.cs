using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;

/// <summary>
/// Returns all stamp cards that are incompleted.
/// </summary>
/// <param name="Team"></param>
/// <param name="AccountingYear"></param>
public record GetIncompletedStampCardsQuery(string Team, int AccountingYear, int NumberOfRequiredStamps)
    : IRequest<GetIncompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, ITeamQuery;