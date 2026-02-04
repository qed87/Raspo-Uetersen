using FluentResults;
using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.DeleteStampCard;

/// <inheritdoc />
public class DeleteStampCardCommandHandler(
    IServiceProvider serviceProvider, 
    IAuthorizationService authorizationService, 
    IHttpContextAccessor httpContextAccessor,
    ILogger<DeleteStampCardCommandHandler> logger) 
    : OnlyTeamCoachHandlerBase<DeleteStampCardCommand, Unit>(
        serviceProvider, authorizationService, httpContextAccessor, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Delete stamp card with Id '{StampCardId}'.", context.Command.Id);
        await context.Model.DeleteStampCard(context.Command.Id);
    }
}