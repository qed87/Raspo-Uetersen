using FluentResults;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.RemovePlayer;

/// <inheritdoc />
[UsedImplicitly]
public class RemoveMemberCommandHandler(IServiceProvider serviceProvider, IAuthorizationService authorizationService, 
    IHttpContextAccessor httpContextAccessor, ILogger<RemoveMemberCommandHandler> logger) 
    : OnlyTeamCoachHandlerBase<RemoveMemberCommand, Guid>(serviceProvider, authorizationService, httpContextAccessor, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Remove player '{PlayerId}' from '{Team}'.", context.Command.MemberId, context.Command.Team);
        var result = await context.Model.RemovePlayerAsync(context.Command.MemberId);
        context.SetResult(result);
    }
}