using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.CreateTeamStampCardsForAccountingYear;

/// <inheritdoc />
public class CreateTeamStampCardsForAccountingYearsCommandHandler(
    IServiceProvider serviceProvider,
    IAuthorizationService authorizationService,
    IHttpContextAccessor httpContextAccessor,
    IValidator<CreateTeamStampCardsForAccountingYearsCommand> validator,
    ILogger<CreateTeamStampCardsForAccountingYearsCommandHandler> logger)
    : OnlyTeamCoachHandlerBase<CreateTeamStampCardsForAccountingYearsCommand, Unit>(serviceProvider, 
        authorizationService, httpContextAccessor, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(
        ICommandExecutionContext context)
    {
        var validationResult = await validator.ValidateAsync(context.Command);
        if (!validationResult.IsValid)
        {
            context.SetResult(validationResult.ToResult());
            return;
        }
        
        logger.LogInformation("Create missing stamp cards for accounting year '{AccountingYear}'.", 
            context.Command.AccountingYear);
        var result = await context.Model.CreateNewAccountingYearAsync(context.Command.AccountingYear);
        context.SetResult(result);
    }
}