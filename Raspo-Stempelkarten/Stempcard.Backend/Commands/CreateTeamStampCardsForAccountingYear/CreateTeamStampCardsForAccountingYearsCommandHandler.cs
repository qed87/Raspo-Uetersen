using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;

/// <inheritdoc />
public class CreateTeamStampCardsForAccountingYearsCommandHandler(IServiceProvider serviceProvider)
    : CommandHandlerBase<CreateTeamStampCardsForAccountingYearsCommand, Unit>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Unit>> ApplyCommandToModel(CreateTeamStampCardsForAccountingYearsCommand command, ITeamAggregate model)
    {
        return await model.CreateNewAccountingYearAsync(command.AccountingYear);
    }
}