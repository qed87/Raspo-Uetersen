using FluentResults;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.CreateStampCard;

/// <inheritdoc />
public class CreateStampCardCommandHandler(
    IServiceProvider serviceProvider,
    IAuthorizationService authorizationHandler,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CreateStampCardCommandHandler> logger) 
    : OnlyTeamCoachHandlerBase<CreateStampCardCommand, Guid>(serviceProvider, authorizationHandler, 
        httpContextAccessor, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Add stamp card for player '{PlayerId}' and accounting year '{AccountingYear}'.", 
            context.Command.PlayerId, context.Command.AccountingYear);
        var result = await context.Model.AddStampCardAsync(context.Command.PlayerId, context.Command.AccountingYear);
        context.SetResult(result);
    }
}