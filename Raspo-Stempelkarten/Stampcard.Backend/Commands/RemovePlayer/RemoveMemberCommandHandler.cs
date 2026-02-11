using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.RemovePlayer;

/// <inheritdoc />
[UsedImplicitly]
public class RemoveMemberCommandHandler(IServiceProvider serviceProvider, ILogger<RemoveMemberCommandHandler> logger) 
    : CommandHandlerBase<RemoveMemberCommand, Guid>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Remove player '{PlayerId}' from '{Team}'.", context.Command.MemberId, context.Command.Team);
        var result = await context.Model.RemovePlayerAsync(context.Command.MemberId);
        context.SetResult(result);
    }
}