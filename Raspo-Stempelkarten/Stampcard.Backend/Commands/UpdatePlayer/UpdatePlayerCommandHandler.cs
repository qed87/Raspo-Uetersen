using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;

namespace StampCard.Backend.Commands.UpdatePlayer;

/// <inheritdoc />
[UsedImplicitly]
public class UpdatePlayerCommandHandler(
    IServiceProvider serviceProvider,
    IValidator<UpdatePlayerCommand> validator,
    ILogger<UpdatePlayerCommandHandler> logger) 
    : CommandHandlerBase<UpdatePlayerCommand, Unit>(serviceProvider, logger)
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
        
        logger.LogInformation("Update player with FirstName = {FirstName}, LastName = {LastName}, Birthdate = {Birthdate}, Birthplace = {Birthplace}, Active = {Active}.", 
            context.Command.FirstName, context.Command.LastName, context.Command.Birthdate, context.Command.Birthplace, context.Command.Active);
        var result = await context.Model.UpdatePlayerAsync(context.Command.Id, context.Command.FirstName, 
            context.Command.LastName, context.Command.Birthdate, context.Command.Birthplace, context.Command.Active);
        context.SetResult(result);
    }
}