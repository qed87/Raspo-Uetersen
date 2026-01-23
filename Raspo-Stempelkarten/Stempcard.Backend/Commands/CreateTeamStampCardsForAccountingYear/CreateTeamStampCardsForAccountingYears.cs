using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;

public record CreateTeamStampCardsForAccountingYears(string Team, int AccountingYear) 
    : IRequest<CreateTeamStampCardsForAccountingYears, Task<Result<CreateTeamStampCardsForAccountingYearsResponse>>>;