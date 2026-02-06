using FluentResults;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.RemoveCoach;

/// <inheritdoc />
[UsedImplicitly]
public class RemoveCoachCommandHandler(IServiceProvider serviceProvider, IAuthorizationService authorizationService, 
    IHttpContextAccessor httpContextAccessor, ILogger<RemoveCoachCommandHandler> logger) : 
    CommandHandlerBase<RemoveCoachCommand, Unit>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Remove coach '{Coach}' from '{Team}'.", context.Command.Email, context.Command.Team);
        var result = await context.Model.RemoveCoach(context.Command.Email);
        context.SetResult(result);
    }
}