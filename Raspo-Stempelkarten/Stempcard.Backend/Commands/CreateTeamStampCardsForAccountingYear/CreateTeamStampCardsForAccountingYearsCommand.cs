using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;

/// <summary>
/// Automatically creates stamp cards for each member of a team for a given accounting year. 
/// </summary>
public record CreateTeamStampCardsForAccountingYearsCommand(string Team, int AccountingYear) 
    : IRequest<CreateTeamStampCardsForAccountingYearsCommand, Task<Result<Unit>>>, ITeamCommand;