using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.StampStampCard;

/// <inheritdoc />
public class StampStampCardCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<StampStampCardCommand, Guid>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Guid>> ApplyCommandToModel(StampStampCardCommand command, ITeamAggregate model)
    {
        return await model.StampStampCardAsync(command.StampCardId, command.Reason);
    }
}