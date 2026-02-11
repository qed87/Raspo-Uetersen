using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using StampCard.Backend.Model;

namespace StampCard.Backend.Commands.DeleteStampCard;

/// <inheritdoc />
public class DeleteStampCardCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<DeleteStampCardCommandHandler> logger) 
    : CommandHandlerBase<DeleteStampCardCommand, Unit>(
        serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Delete stamp card with Id '{StampCardId}'.", context.Command.Id);
        await context.Model.DeleteStampCardAsync(context.Command.Id);
    }
}