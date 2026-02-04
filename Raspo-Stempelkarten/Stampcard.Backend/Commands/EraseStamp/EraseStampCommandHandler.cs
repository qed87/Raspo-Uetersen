using FluentResults;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.EraseStamp;

/// <inheritdoc />
public class EraseStampCommandHandler(IServiceProvider serviceProvider, IAuthorizationService authorizationService, 
    IHttpContextAccessor httpContextAccessor, ILogger<EraseStampCommandHandler> logger) 
    : OnlyTeamCoachHandlerBase<EraseStampCommand, Guid>(serviceProvider, authorizationService, httpContextAccessor, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Erase stamp with Id = '{Id}'  from '{StampCardId}'.", context.Command.Id, context.Command.StampCardId);
        var result = await context.Model.EraseStampAsync(context.Command.StampCardId, context.Command.Id);
        context.SetResult(result);
    }
}