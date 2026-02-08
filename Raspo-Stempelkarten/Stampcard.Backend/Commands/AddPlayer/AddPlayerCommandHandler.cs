using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.AddPlayer;

/// <inheritdoc />
[UsedImplicitly]
public class AddPlayerCommandHandler(
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorizationService,
    IValidator<AddPlayerCommand> validator,
    ILogger<AddPlayerCommandHandler> logger) 
    : OnlyTeamCoachHandlerBase<AddPlayerCommand, Guid>(serviceProvider, authorizationService, httpContextAccessor, logger)
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
        
        logger.LogInformation("Add player with FirstName = {FirstName}, LastName = {LastName}, Birthdate = {Birthdate}, Birthplace = {Birthplace}.", 
            context.Command.FirstName, context.Command.LastName, context.Command.Birthdate, context.Command.Birthplace);
        var result = await context.Model.AddPlayerAsync(context.Command.FirstName, context.Command.LastName, 
            context.Command.Birthdate, context.Command.Birthplace);
        context.SetResult(result);
    }
}