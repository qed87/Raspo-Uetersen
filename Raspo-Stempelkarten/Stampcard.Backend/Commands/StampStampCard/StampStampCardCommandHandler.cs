using FluentResults;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.StampStampCard;

/// <inheritdoc />
public class StampStampCardCommandHandler(IServiceProvider serviceProvider, IAuthorizationService  authorizationService, 
    IHttpContextAccessor  httpContextAccessor, ILogger<StampStampCardCommandHandler> logger) 
    : CommandHandlerBase<StampStampCardCommand, Guid>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Stamp stamp card '{StampCardId}'.", context.Command.StampCardId);
        var result = await context.Model.StampAsync(context.Command.StampCardId, context.Command.Reason);
        context.SetResult(result);
    }
}