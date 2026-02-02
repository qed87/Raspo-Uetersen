using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.EraseStamp;

/// <inheritdoc />
public class EraseStampCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<EraseStampCommand, Guid>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Guid>> ApplyCommandToModel(EraseStampCommand command, ITeamAggregate model)
    {
        return await model.EraseStampAsync(command.StampCardId, command.StampId);
    }
}