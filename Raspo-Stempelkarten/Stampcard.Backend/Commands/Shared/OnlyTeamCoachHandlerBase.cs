using FluentResults;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.Shared;

/// <summary>
/// Command handler that allows only access for team coaches. 
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="authorizationService">The authorization service.</param>
/// <param name="httpContextAccessor">The http context accessor.</param>
/// <param name="logger">The logger.</param>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class OnlyTeamCoachHandlerBase<TCommand, TResult>(
    IServiceProvider serviceProvider,
    IAuthorizationService authorizationService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CommandHandlerBase<TCommand, TResult>> logger) 
    : CommandHandlerBase<TCommand, TResult>(serviceProvider, logger) where TCommand : class, ITeamCommand
{
    /// <summary>
    /// Checks that the current user is a team coach.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <returns></returns>
    protected override async Task PrepareCommandAsync(ICommandExecutionContext context)
    {
        await base.PrepareCommandAsync(context);
        var user = httpContextAccessor.HttpContext!.User;
        logger.LogTrace("Verify that user is a team coach.");
        var authorizationResult = await authorizationService.AuthorizeAsync(
            user, 
            context.Model, 
            "TeamCoachOnly");
        if (!authorizationResult.Succeeded)
        {
            logger.LogDebug("User '{UserName}' is not a team coach.", user.Identity?.Name ?? "Anonymous");
            context.SetResult(authorizationResult.ToResult());
            return;
        } 
        
        logger.LogDebug("User '{UserName}' is a valid team coach.", user.Identity?.Name ?? "Anonymous");
    }
}