using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.AddCoach;

/// <inheritdoc />
[UsedImplicitly]
public class AddCoachCommandHandler(
    IServiceProvider serviceProvider,
    IValidator<AddCoachCommand> validator,
    ILogger<AddCoachCommandHandler> logger) 
    : CommandHandlerBase<AddCoachCommand, Unit>(serviceProvider, logger)
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
        
        logger.LogDebug("Add coach with email '{Email}'.", context.Command.Email);
        var result = await context.Model.AddCoachAsync(context.Command.Email);
        context.SetResult(result);
    }
}