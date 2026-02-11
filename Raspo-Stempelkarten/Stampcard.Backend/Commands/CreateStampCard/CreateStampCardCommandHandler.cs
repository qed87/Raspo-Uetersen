using FluentResults;
using FluentValidation;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.CreateStampCard;

/// <inheritdoc />
public class CreateStampCardCommandHandler(
    IServiceProvider serviceProvider,
    IValidator<CreateStampCardCommand> validator,
    ILogger<CreateStampCardCommandHandler> logger) 
    : CommandHandlerBase<CreateStampCardCommand, Unit>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        var validationResult = await validator.ValidateAsync(context.Command);
        if (!validationResult.IsValid)
        {
            context.SetResult(validationResult.ToResult());
            return;
        }

        if (string.Equals(context.Command.Flag, "auto", StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Add stamp cards for accounting year '{AccountingYear}'.", 
                context.Command.AccountingYear);
            var response = await context.Model.AddStampCardsAsync(context.Command.AccountingYear);
            if(response.IsFailed) context.SetResult(response);
        }
        else if (string.Equals(context.Command.Flag, "manual", StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Add stamp card for player '{PlayerId}' and accounting year '{AccountingYear}'.", 
                context.Command.PlayerId, context.Command.AccountingYear);
            var response = await context.Model.AddStampCardAsync(context.Command.PlayerId, context.Command.AccountingYear);
            if(response.IsFailed) context.SetResult(response.ToResult());
        }
        else
        {
            context.SetResult(Result.Fail($"Unbekannte command Flag '{context.Command.Flag}'."));
        }
    }
}