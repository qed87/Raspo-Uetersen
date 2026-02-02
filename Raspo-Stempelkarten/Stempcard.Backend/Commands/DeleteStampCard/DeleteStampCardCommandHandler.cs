using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;

/// <inheritdoc />
public class DeleteStampCardCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<DeleteStampCardCommand, Unit>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Unit>> ApplyCommandToModel(DeleteStampCardCommand command, ITeamAggregate model)
    {
        return (await model.DeleteTeamAsync()).ToResult();
    }
}