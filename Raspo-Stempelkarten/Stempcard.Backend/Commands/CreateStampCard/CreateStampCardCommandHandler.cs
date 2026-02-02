using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.CreateStampCard;

/// <inheritdoc />
public class CreateStampCardCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<CreateStampCardCommand, Guid>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Guid>> ApplyCommandToModel(CreateStampCardCommand command, ITeamAggregate model)
    {
        return await model.AddStampCardAsync(command.MemberId, command.AccountingYear);
    }
}